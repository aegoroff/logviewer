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