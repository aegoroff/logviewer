// Created by: egr
// Created at: 12.07.2015
// © 2012-2015 Alexander Egorov

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace logviewer.engine.grammar
{
    [Serializable]
    internal class GrokSyntaxException : Exception
    {

        private readonly string message;

        /// <summary>Creates new empty exception instance.</summary>
        public GrokSyntaxException()
        {
        }

        /// <summary>Creates new exception instance.</summary>
        /// <param name="message">Error message.</param>
        public GrokSyntaxException(string message)
        {
            this.message = message;
        }

        /// <summary>Initializes a new instance of the GrokSyntaxException class with serialized data.</summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream, and provides an additional caller-defined context.</param>
        /// <exception cref="ArgumentNullException">Occurs in case of info parameter is null.</exception>
        protected GrokSyntaxException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            if ((info == null))
            {
                throw new ArgumentNullException("info");
            }
            this.message = info.GetString("message");
        }

        /// <summary>Creates new exception instance with keeping inner exception.</summary>
        /// <param name="message">Error message.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public GrokSyntaxException(string message, Exception innerException) :
            base(message, innerException)
        {
            this.message = message;
        }

        public override string Message
        {
            get
            {
                return this.message;
            }
        }

        /// <summary>Sets the SerializationInfo with information about the exception.</summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream, and provides an additional caller-defined context.</param>
        /// <exception cref="ArgumentNullException">Occurs in case of info parameter is null.</exception>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if ((info == null))
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("message", this.message);
            base.GetObjectData(info, context);
        }
    }
}