using System;

namespace logviewer.logic.fsm
{
    public partial class SolidMachine<TTrigger>
    {
        public class TriggerConfiguration
        {
            private readonly StateConfiguration owningStateConfiguration;

            public TriggerConfiguration(TTrigger trigger, StateConfiguration owningStateConfiguration)
            {
                this.Trigger = trigger;
                this.owningStateConfiguration = owningStateConfiguration;
            }

            public TriggerConfiguration(TTrigger trigger, Func<bool> guardClause, StateConfiguration owningStateConfiguration)
            {
                this.GuardClause = guardClause;
                this.Trigger = trigger;
                this.owningStateConfiguration = owningStateConfiguration;
            }


            internal TTrigger Trigger { get; }

            internal StateConfiguration TargetState { get; private set; }

            internal Func<bool> GuardClause { get; }

            public StateConfiguration GoesTo<TTargetState>() where TTargetState : ISolidState
            {
                this.TargetState = this.owningStateConfiguration.OwningMachine.State<TTargetState>();

                // Return the correct StateConfiguration
                var machine = this.owningStateConfiguration.OwningMachine;
                return machine.stateConfigurations[this.owningStateConfiguration.StateType];
            }
        }
    }
}