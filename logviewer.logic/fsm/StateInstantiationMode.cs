// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 24.11.2016
// © 2012-2017 Alexander Egorov

using logviewer.logic.Annotations;

namespace logviewer.logic.fsm
{
    /// <summary>
    ///     Enumeration of possible state instantiation modes.
    /// </summary>
    [PublicAPI]
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