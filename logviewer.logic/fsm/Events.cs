// Created by: egr
// Created at: 11.12.2015
// © 2012-2016 Alexander Egorov

using System;

namespace logviewer.logic.fsm
{
    /// <summary>
    ///     Delegate for the SolidMachine.Transitioned event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void TransitionedEventHandler(object sender, TransitionedEventArgs e);

    /// <summary>
    ///     EventArgs for the SolidMachine.Transitioned event.
    /// </summary>
    public class TransitionedEventArgs : EventArgs
    {
        public TransitionedEventArgs(Type sourceState, Type targetState)
        {
            this.SourceState = sourceState;
            this.TargetState = targetState;
        }

        /// <summary>
        ///     The source state of the transition.
        /// </summary>
        public Type SourceState { get; }

        /// <summary>
        ///     The target state of the transition.
        /// </summary>
        public Type TargetState { get; }
    }
}