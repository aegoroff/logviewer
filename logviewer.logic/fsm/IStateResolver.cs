// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 11.12.2015
// © 2012-2016 Alexander Egorov

using System;

namespace logviewer.logic.fsm
{
    /// <summary>
    ///     Implements a resolver to instantiate states for the SolidMachine.
    /// </summary>
    public interface IStateResolver
    {
        // Methods

        /// <summary>
        ///     Instantiates a state object the specified type.
        /// </summary>
        /// <param name="stateType"></param>
        /// <returns></returns>
        ISolidState ResolveState(Type stateType);
    }
}