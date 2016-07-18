// Created by: egr
// Created at: 11.12.2015
// © 2012-2016 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using logviewer.logic.Annotations;

namespace logviewer.logic.fsm
{
    public partial class SolidMachine<TTrigger>
    {
        private const int DefaultStatehistoryTrimThreshold = 100;
        private const int MinStatehistoryTrimThreshold = 10;
        private const double StatehistoryTrimPercentage = 0.1; // Trim 10% of state history

        private readonly object queueLockObject = new object();

        private readonly Dictionary<Type, StateConfiguration> stateConfigurations;
        private readonly List<StateConfiguration> stateHistory;
        private readonly object stateHistoryLockObject = new object();
        private readonly List<Action> transitionQueue;

        private StateConfiguration currentState;

        private StateConfiguration initialState;

        private bool initialStateConfigured;

        private Action<Type, TTrigger> invalidTriggerHandler;
        private bool isProcessingQueue;
        private bool isStarted;
        private int stateHistoryTrimThreshold;

        private StateInstantiationMode stateInstantiationMode;

        private bool stateResolverRequired;

        public SolidMachine()
        {
            this.stateConfigurations = new Dictionary<Type, StateConfiguration>();
            this.transitionQueue = new List<Action>();
            this.stateHistory = new List<StateConfiguration>();

            this.stateHistoryTrimThreshold = DefaultStatehistoryTrimThreshold;
        }

        [PublicAPI]
        public SolidMachine(object context) : this()
        {
            this.Context = context;
        }

        [PublicAPI]
        public SolidMachine(object context, IStateResolver stateResolver) : this(context)
        {
            this.Context = context;
            this.StateResolver = stateResolver;
        }

        // Properties

        /// <summary>
        ///     The type of the machines initial state.
        /// </summary>
        [PublicAPI]
        public Type InitialState => this.initialState.StateType;

        /// <summary>
        ///     Returns the state that the state machine is currently in.
        /// </summary>
        [PublicAPI]
        public ISolidState CurrentState => this.currentState?.StateInstance;

        /// <summary>
        ///     A list of the triggers that are valid to use on the current state.
        ///     If the machine hasn't been started yet, this list is empty.
        /// </summary>
        [PublicAPI]
        public List<TTrigger> ValidTriggers => this.GetValidTriggers();

        /// <summary>
        ///     An arbitrary object that will be passed on to the states in their entry and exit methods.
        ///     If no context is defined, the state machine instance will be used as context.
        /// </summary>
        [PublicAPI]
        public object Context { get; set; }

        /// <summary>
        ///     The resolver for state machine states. If this is not specified the standard
        ///     .NET activator is used and all states must then have parameterless constructors.
        /// </summary>
        [PublicAPI]
        public IStateResolver StateResolver { get; set; }

        /// <summary>
        ///     Controls whether state class instances should be Singletons or if they should be instantiated
        ///     for each transition. Default value is Singleton.
        /// </summary>
        [PublicAPI]
        public StateInstantiationMode StateInstantiationMode
        {
            get { return this.stateInstantiationMode; }
            set
            {
                // This is only OK to change 
                if (this.isStarted)
                {
                    throw new SolidStateException(SolidStateConstants.ErrorStateMachineIsStarted,
                        "The StateInstantiationMode must be set before the state machine is started!");
                }
                this.stateInstantiationMode = value;
            }
        }

        /// <summary>
        ///     Returns an array of the states the state machine has been in. The last state is at index 0.
        ///     The current state is not part of the list.
        /// </summary>
        public Type[] StateHistory
        {
            get
            {
                lock (this.stateHistoryLockObject)
                {
                    return this.stateHistory.Select(x => x.StateType).ToArray();
                }
            }
        }

        /// <summary>
        ///     The number of entries that will be kept in the state history before an automatic
        ///     trim is performed.
        /// </summary>
        [PublicAPI]
        public int StateHistoryTrimThreshold
        {
            get { return this.stateHistoryTrimThreshold; }
            set
            {
                // Can't set a too low value
                if (value < MinStatehistoryTrimThreshold)
                {
                    value = MinStatehistoryTrimThreshold;
                }

                this.stateHistoryTrimThreshold = value;

                // If the new value is lower we may need a trim right away
                this.AddStateToHistory(null);
            }
        }

        // Private methods

        /// <summary>
        ///     Throws an exception if the state machine hasn't been started yet.
        /// </summary>
        private void ThrowOnNotStarted()
        {
            if (!this.isStarted)
            {
                throw new SolidStateException(SolidStateConstants.ErrorStateMachineNotStarted, "State machine is not started!");
            }
        }

        /// <summary>
        ///     Enters a new state (if there is one) and raises the OnTransitioned event.
        /// </summary>
        private void EnterNewState(Type previousStateType, StateConfiguration state)
        {
            this.currentState = state;

            Type currentStateType = null;

            // Are we entering a new state?
            if (this.currentState != null)
            {
                currentStateType = this.currentState.StateType;
                this.currentState.Enter();
            }

            // Raise an event about the transition
            this.OnTransitioned(new TransitionedEventArgs(previousStateType, currentStateType));
        }

        /// <summary>
        ///     Exits the current state and returns the Type of it.
        /// </summary>
        /// <returns></returns>
        private Type ExitCurrentState(bool addToHistory)
        {
            if (this.currentState == null)
            {
                return null;
            }
            this.currentState.Exit();

            // Record it in the history
            if (addToHistory)
            {
                this.AddStateToHistory(this.currentState);
            }

            var stateType = this.currentState.StateType;
            this.currentState = null;
            return stateType;
        }

        /// <summary>
        ///     Handles the processing of a trigger, calculating if it is valid and which target state we should go to.
        /// </summary>
        /// <param name="trigger"></param>
        private void DoTrigger(TTrigger trigger)
        {
            // Find all trigger configurations with a matching trigger
            var triggers = this.currentState.TriggerConfigurations.Where(x => x.Trigger.Equals(trigger)).ToList();

            // No trigger configs found?
            if (triggers.Count == 0)
            {
                // Do we have a handler for the situation?
                if (this.invalidTriggerHandler == null)
                {
                    throw new SolidStateException(SolidStateConstants.ErrorTriggerNotValidForState,
                        $"Trigger {trigger} is not valid for state {this.currentState.StateType.Name}!");
                }
                // Let the handler decide what to do
                this.invalidTriggerHandler(this.currentState.StateType, trigger);
            }
            else
            {
                // Is it a single, unguarded trigger?
                if (triggers[0].GuardClause == null)
                {
                    var previousStateType = this.ExitCurrentState(true);
                    this.EnterNewState(previousStateType, triggers[0].TargetState);
                }
                else
                {
                    // First exit the current state, it may affect the evaluation of the guard clauses
                    var previousStateType = this.ExitCurrentState(true);

                    TriggerConfiguration matchingTrigger = null;
                    foreach (var tr in triggers.Where(tr => tr.GuardClause()))
                    {
                        if (matchingTrigger != null)
                        {
                            throw new SolidStateException(SolidStateConstants.ErrorMultipleGuardClausesAreTrue,
                                $"State {previousStateType.Name}, trigger {trigger} has multiple guard clauses that simultaneously evaulate to True!");
                        }
                        matchingTrigger = tr;
                    }

                    // Did we find a matching trigger?
                    if (matchingTrigger == null)
                    {
                        throw new SolidStateException(SolidStateConstants.ErrorNoGuardClauseIsTrue,
                            $"State {previousStateType.Name}, trigger {trigger} has no guard clause that evaulate to True!");
                    }

                    // Queue up the transition
                    this.EnterNewState(previousStateType, matchingTrigger.TargetState);
                }
            }
        }

        private void AddStateToHistory(StateConfiguration state)
        {
            lock (this.stateHistoryLockObject)
            {
                if (state != null)
                {
                    this.stateHistory.Insert(0, state);
                }

                // Time to trim it?
                if (this.stateHistory.Count <= this.stateHistoryTrimThreshold)
                {
                    return;
                }
                var trimValue = (int) (this.stateHistoryTrimThreshold * (1.0 - StatehistoryTrimPercentage));
                while (this.stateHistory.Count > trimValue)
                {
                    this.stateHistory.RemoveAt(trimValue);
                }
            }
        }

        /// <summary>
        ///     Loops through the transition queue until it is empty, executing the queued
        ///     calls to the ExecuteTransition method.
        /// </summary>
        private void ProcessTransitionQueue()
        {
            if (this.isProcessingQueue)
            {
                return;
            }

            try
            {
                this.isProcessingQueue = true;

                do
                {
                    Action nextAction;

                    // Lock queue during readout of next action
                    lock (this.queueLockObject)
                    {
                        if (this.transitionQueue.Count == 0)
                        {
                            return;
                        }

                        nextAction = this.transitionQueue[0];
                        this.transitionQueue.RemoveAt(0);
                    }

                    nextAction?.Invoke();
                } while (true);
            }
            finally
            {
                this.isProcessingQueue = false;
            }
        }

        /// <summary>
        ///     Sets the initial state of the state machine.
        /// </summary>
        private void SetInitialState(StateConfiguration initialStateConfiguration)
        {
            this.initialState = initialStateConfiguration;
            this.initialStateConfigured = true;
        }

        /// <summary>
        ///     Creates an instance of a specified state type, either through .NET activation
        ///     or through a defined state resolver.
        /// </summary>
        private ISolidState InstantiateState(Type stateType)
        {
            // Do we have a state resolver?
            if (this.StateResolver == null)
            {
                return (ISolidState) Activator.CreateInstance(stateType);
            }
            var instance = this.StateResolver.ResolveState(stateType);
            if (instance == null)
            {
                throw new SolidStateException(SolidStateConstants.ErrorStateResolverCouldNotResolveType,
                    $"State resolver could not resolve a state for type '{stateType.Name}'!");
            }
            return instance;
        }

        /// <summary>
        ///     Checks if a state resolver will be required on state machine startup.
        /// </summary>
        /// <param name="stateType"></param>
        private void HandleResolverRequired(Type stateType)
        {
            // A state resolver is required if a configured state has no parameterless constructor
            this.stateResolverRequired = stateType.GetConstructor(Type.EmptyTypes) == null;
        }

        /// <summary>
        ///     Gets the object that should be used as state context.
        /// </summary>
        /// <returns></returns>
        private object GetContext()
        {
            return this.Context ?? this;
        }

        /// <summary>
        ///     Returns a list of all triggers that are valid to use on the current state.
        /// </summary>
        /// <returns></returns>
        private List<TTrigger> GetValidTriggers()
        {
            if (!this.isStarted || (this.currentState == null))
            {
                return new List<TTrigger>();
            }

            // Return a distinct list (no duplicates) of triggers
            return this.currentState.TriggerConfigurations.Select(x => x.Trigger).Distinct().ToList();
        }

        // Protected methods

        /// <summary>
        ///     Raises the Transitioned event.
        /// </summary>
        protected virtual void OnTransitioned(TransitionedEventArgs eventArgs)
        {
            this.Transitioned?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Raised when the state machine has transitioned from one state to another.
        /// </summary>
        public event TransitionedEventHandler Transitioned;

        /// <summary>
        ///     Defines a state that should be configured.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public StateConfiguration State<TState>() where TState : ISolidState
        {
            var type = typeof (TState);
            // Does the state have a parameterless constructor? Otherwise a state resolver is required
            this.HandleResolverRequired(type);

            // Does a configuration for this state exist already?
            StateConfiguration configuration;
            if (this.stateConfigurations.ContainsKey(typeof (TState)))
            {
                configuration = this.stateConfigurations[typeof (TState)];
            }
            else
            {
                configuration = new StateConfiguration(type, this);

                // If this is the first state that is added, it becomes the initial state
                if (this.stateConfigurations.Count == 0)
                {
                    this.initialState = configuration;
                }

                this.stateConfigurations.Add(type, configuration);
            }

            return configuration;
        }

        /// <summary>
        ///     Starts the state machine by going to the initial state.
        /// </summary>
        public void Start()
        {
            if (this.initialState == null)
            {
                throw new SolidStateException(SolidStateConstants.ErrorNoStatesHaveBeenConfigured, "No states have been configured!");
            }

            // If there are states that has no parameterless constructor, we must have set the StateResolver property.
            if (this.stateResolverRequired && (this.StateResolver == null))
            {
                throw new SolidStateException(SolidStateConstants.ErrorStatesNeedParameterlessConstructor,
                    "One or more configured states has no parameterless constructor. Add such constructors or make sure that the StateResolver property is set!");
            }

            this.isStarted = true;

            // Enter the initial state
            this.EnterNewState(null, this.initialState);
        }

        /// <summary>
        ///     Stops the state machine by exiting the current state without entering a new one.
        /// </summary>
        public void Stop()
        {
            // Ignore this call if the machine hasn't been started yet
            if (!this.isStarted)
            {
                return;
            }

            try
            {
                // Empty the queue
                lock (this.queueLockObject)
                {
                    this.transitionQueue.Clear();
                }

                // Exit the current state and raise an event about it
                var previousStateType = this.ExitCurrentState(false);
                this.OnTransitioned(new TransitionedEventArgs(previousStateType, null));
            }
            finally
            {
                this.isStarted = false;
            }
        }

        /// <summary>
        ///     Sets the invalid trigger handler that will be called when a trigger is used
        ///     that isn't valid for the current state. If no handler is specified, an
        ///     exception will be thrown.
        /// </summary>
        public void OnInvalidTrigger(Action<Type, TTrigger> invalidHandler)
        {
            this.invalidTriggerHandler = invalidHandler;
        }

        /// <summary>
        ///     Adds a request to fire a trigger to the processing queue.
        /// </summary>
        /// <param name="trigger"></param>
        public void Trigger(TTrigger trigger)
        {
            Trace.WriteLine(trigger);
            this.ThrowOnNotStarted();

            // Queue it up
            lock (this.queueLockObject)
            {
                var localTrigger = trigger;
                this.transitionQueue.Add(() => this.DoTrigger(localTrigger));
            }

            this.ProcessTransitionQueue();
        }

        /// <summary>
        ///     Moves the state machine back to the previous state, ignoring valid triggers, guard clauses etc.
        /// </summary>
        public void GoBack()
        {
            StateConfiguration targetState;
            Type previousStateType;

            lock (this.stateHistoryLockObject)
            {
                // If the history is empty, we just ignore the call
                if (this.stateHistory.Count == 0)
                {
                    return;
                }

                previousStateType = this.ExitCurrentState(false);

                targetState = this.stateHistory[0];
                this.stateHistory.RemoveAt(0);
            }

            if (targetState != null)
            {
                this.EnterNewState(previousStateType, targetState);
            }
        }
    }

    /// <summary>
    ///     Enumeration of possible state instantiation modes.
    /// </summary>
    public enum StateInstantiationMode
    {
        /// <summary>
        ///     The state class is instantiated the first time it is used. All following
        ///     transitions into that state will use the same instance.
        /// </summary>
        Singleton,

        /// <summary>
        ///     The target state of a transition is instantiated on each transition.
        /// </summary>
        PerTransition
    }
}