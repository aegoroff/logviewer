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