using System;

namespace logviewer.logic.fsm
{
    /// <summary>
    ///     Base exception for the Solid.State framework.
    /// </summary>
    public class SolidStateException : Exception
    {
        // Constructors

        public SolidStateException(int errorId, string message) : base(message)
        {
            this.ErrorId = errorId;
        }

        public SolidStateException(int errorId, string message, Exception innerException) : base(message, innerException)
        {
            this.ErrorId = errorId;
        }

        // Properties

        /// <summary>
        ///     Gets the id associated with error that caused the exception.
        /// </summary>
        public int ErrorId { get; private set; }
    }
}