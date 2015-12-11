// Created by: egr
// Created at: 11.12.2015
// © 2012-2015 Alexander Egorov

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