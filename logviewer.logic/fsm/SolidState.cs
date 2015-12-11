namespace logviewer.logic.fsm
{
    /// <summary>
    ///     A simple base class that can be used when creating state machine states. Since the interface is implemented
    ///     with virtual methods in this class, subclasses can choose which methods to override.
    /// </summary>
    public abstract class SolidState : ISolidState
    {
        public void Entering(object context)
        {
            this.DoEntering(context);
        }

        public void Exiting(object context)
        {
            this.DoExiting(context);
        }

        protected virtual void DoEntering(object context)
        {
            // No code
        }

        protected virtual void DoExiting(object context)
        {
            // No code
        }
    }
}