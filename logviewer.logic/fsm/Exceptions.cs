// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 11.12.2015
// © 2012-2016 Alexander Egorov

using System;
using logviewer.logic.Annotations;

namespace logviewer.logic.fsm
{
    /// <summary>
    ///     Base exception for the Solid.State framework.
    /// </summary>
    public class SolidStateException : Exception
    {
        // Constructors
        [PublicAPI]
        public SolidStateException(int errorId, string message) : base(message)
        {
            this.ErrorId = errorId;
        }

        [PublicAPI]
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