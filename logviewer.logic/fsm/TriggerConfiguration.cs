// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 11.12.2015
// © 2012-2016 Alexander Egorov

using System;
using logviewer.logic.Annotations;

namespace logviewer.logic.fsm
{
    public partial class SolidMachine<TTrigger>
    {
        public class TriggerConfiguration
        {
            private readonly StateConfiguration owningStateConfiguration;

            [PublicAPI]
            public TriggerConfiguration(TTrigger trigger, StateConfiguration owningStateConfiguration)
            {
                this.Trigger = trigger;
                this.owningStateConfiguration = owningStateConfiguration;
            }

            [PublicAPI]
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