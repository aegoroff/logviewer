using System;
using System.Collections.Generic;
using System.Linq;

namespace logviewer.logic.fsm
{
    public partial class SolidMachine<TTrigger>
    {
        public class StateConfiguration
        {

            public StateConfiguration(Type stateType, SolidMachine<TTrigger> owningMachine)
            {
                this.StateType = stateType;
                this.OwningMachine = owningMachine;
                this.TriggerConfigurations = new List<TriggerConfiguration>();
            }

            internal SolidMachine<TTrigger> OwningMachine { get; }

            internal List<TriggerConfiguration> TriggerConfigurations { get; }

            internal Type StateType { get; }

            internal ISolidState StateInstance { get; private set; }

            private TriggerConfiguration GetConfigurationByTrigger(TTrigger trigger)
            {
                return this.TriggerConfigurations.FirstOrDefault(x => x.Trigger.Equals(trigger));
            }

            /// <summary>
            ///     Enters a state, creating an instance of it if necessary.
            /// </summary>
            internal void Enter()
            {
                // Should a new instance be created?
                if ((this.StateInstance == null) || (this.OwningMachine.stateInstantiationMode == StateInstantiationMode.PerTransition))
                {
                    this.StateInstance = this.OwningMachine.InstantiateState(this.StateType);
                }

                this.StateInstance.Entering(this.OwningMachine.GetContext());
            }

            /// <summary>
            ///     Exits the state that this configuration is linked to.
            /// </summary>
            internal void Exit()
            {
                if (this.StateInstance == null)
                {
                    return;
                }
                this.StateInstance.Exiting(this.OwningMachine.GetContext());

                // If we're instantiating per transition, we release the reference to the instance
                if (this.OwningMachine.stateInstantiationMode == StateInstantiationMode.PerTransition)
                {
                    this.StateInstance = null;
                }
            }

            // Methods

            public StateConfiguration IsInitialState()
            {
                if (this.OwningMachine.initialStateConfigured)
                {
                    throw new SolidStateException(SolidStateConstants.ErrorMultipleInitialStates,
                        "The state machine cannot have multiple states configured as the initial state!");
                }

                this.OwningMachine.SetInitialState(this);

                return this;
            }

            /// <summary>
            ///     Adds a guardless permitted transition to the state configuration.
            /// </summary>
            /// <param name="trigger">The trigger that this state should accept.</param>
            /// <returns></returns>
            public TriggerConfiguration On(TTrigger trigger)
            {
                var existingConfig = this.GetConfigurationByTrigger(trigger);
                if (existingConfig != null)
                {
                    // Does the existing configuration have a guard clause?
                    if (existingConfig.GuardClause != null)
                    {
                        throw new SolidStateException(SolidStateConstants.ErrorCannotMixGuardedAndGuardlessTransitions,
                            $"State {this.StateType.Name} has at least one guarded transition configured on trigger {trigger} already. " +
                            "A state cannot have both guardless and guarded transitions at the same time!");
                    }
                    throw new SolidStateException(SolidStateConstants.ErrorTriggerAlreadyConfiguredForState,
                        $"Trigger {trigger} has already been configured for state {this.StateType.Name}!");
                }

                var newConfiguration = new TriggerConfiguration(trigger, null, this);
                this.TriggerConfigurations.Add(newConfiguration);

                return newConfiguration;
            }

            public TriggerConfiguration On(TTrigger trigger, Func<bool> guardClause)
            {
                if (guardClause == null)
                {
                    throw new ArgumentNullException(nameof(guardClause));
                }

                var existingConfig = this.GetConfigurationByTrigger(trigger);
                if (existingConfig != null)
                {
                    // It's OK that there are multiple configurations of the same trigger, as long as they all have guard clauses
                    if (existingConfig.GuardClause == null)
                    {
                        throw new SolidStateException(SolidStateConstants.ErrorCannotMixGuardedAndGuardlessTransitions,
                            $"State {this.StateType.Name} has an unguarded transition for trigger {trigger}, you cannot add guarded transitions to this state as well!");
                    }
                }

                var newConfiguration = new TriggerConfiguration(trigger, guardClause, this);
                this.TriggerConfigurations.Add(newConfiguration);

                return newConfiguration;
            }
        }
    }
}