// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 11.12.2015
// © 2012-2017 Alexander Egorov

namespace logviewer.logic.fsm
{
    /// <summary>
    ///     Represents a state implementation for the SolidMachine state machine.
    /// </summary>
    public interface ISolidState
    {
        // Methods

        /// <summary>
        ///     This method is called when the state machine has this state as the
        ///     target of a transition.
        /// </summary>
        /// <param name="context"></param>
        void Entering(object context);

        /// <summary>
        ///     This method is called when the state machine has this state as the
        ///     source of a transition.
        /// </summary>
        /// <param name="context"></param>
        void Exiting(object context);
    }
}