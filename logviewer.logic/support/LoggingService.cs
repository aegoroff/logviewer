// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 21.09.2012
// © 2012-2017 Alexander Egorov

using System;
using System.Threading;
using logviewer.logic.Annotations;
using NLog;

namespace logviewer.logic.support
{
    public class Log : LoggingService
    {
        [PublicAPI]
        internal Log()
            : base(@"logviewer")
        {
        }

        public static Log Instance { get; } = new Log();
    }

    /// <summary>
    ///     Does all logging activity for the application.
    /// </summary>
    public abstract class LoggingService
    {
        private readonly Logger log;

        /// <summary>
        ///     Initializes new <see cref="LoggingService" /> instance
        /// </summary>
        /// <param name="name"> logger name </param>
        protected LoggingService(string name) => this.log = GetLogger(name);

        /// <summary>
        ///     Checks if this logger is enabled for the Trace level.
        /// </summary>
        [PublicAPI]
        public bool IsTraceEnabled => SafeRunner.Run(() => this.log.IsTraceEnabled);

        /// <summary>
        ///     Checks if this logger is enabled for the Debug level.
        /// </summary>
        [PublicAPI]
        public bool IsDebugEnabled => SafeRunner.Run(() => this.log.IsDebugEnabled);

        /// <summary>
        ///     Checks if this logger is enabled for the Info level.
        /// </summary>
        [PublicAPI]
        public bool IsInfoEnabled => SafeRunner.Run(() => this.log.IsInfoEnabled);

        /// <summary>
        ///     Checks if this logger is enabled for the Warn level.
        /// </summary>
        [PublicAPI]
        public bool IsWarnEnabled => SafeRunner.Run(() => this.log.IsWarnEnabled);

        /// <summary>
        ///     Checks if this logger is enabled for the Error level.
        /// </summary>
        [PublicAPI]
        public bool IsErrorEnabled => SafeRunner.Run(() => this.log.IsErrorEnabled);

        /// <summary>
        ///     Checks if this logger is enabled for the Fatal level.
        /// </summary>
        [PublicAPI]
        public bool IsFatalEnabled => SafeRunner.Run(() => this.log.IsFatalEnabled);

        /// <summary>
        ///     Log a message object with the Trace level.
        /// </summary>
        /// <param name="exception"> The exception object to log. </param>
        /// <remarks>
        ///     <para> This method first checks if this logger is TRACE enabled by comparing the level of this logger with the Trace level. If this logger is TRACE enabled, then it converts the message object (passed as parameter) to a string by invoking the appropriate IObjectRenderer. It then proceeds to call all the registered appenders in this logger and also higher in the hierarchy depending on the value of the additivity flag. </para>
        ///     <para> WARNING Notice that passing an Exception to this method will print the name of the Exception but no stack trace. To print a stack trace use the Trace(object, Exception) form instead. </para>
        /// </remarks>
        [PublicAPI]
        public void Trace(Exception exception) => SafeRunner.Run(this.log.Trace, exception, exception.Message, new object[0]);

        /// <summary>
        ///     Log a message object with the Trace level.
        /// </summary>
        /// <param name="message"> The message object to log </param>
        /// <param name="exception"> The exception object to log. </param>
        [PublicAPI]
        public void Trace(object message, Exception exception) => SafeRunner.Run(this.log.Trace, exception, message.ToString(), new object[0]);

        /// <summary>
        ///     Logs a formatted message string with the Trace level.
        /// </summary>
        /// <param name="format"> A String containing zero or more format items </param>
        /// <param name="args"> An Object array containing zero or more objects to format </param>
        [PublicAPI]
        public void TraceFormatted(string format, params object[] args) => this.TraceFormatted(Thread.CurrentThread.CurrentCulture, format, args);

        /// <summary>
        ///     Logs a formatted message string with the Trace level.
        /// </summary>
        /// <param name="provider"> An object that supplies culture-specific formatting information. </param>
        /// <param name="format"> A String containing zero or more format items </param>
        /// <param name="args"> An Object array containing zero or more objects to format </param>
        [PublicAPI]
        public void TraceFormatted(IFormatProvider provider, string format, params object[] args) => SafeRunner.Run(this.log.Trace, provider, format, args);

        /// <summary>
        ///     Log a message object with the Debug level.
        /// </summary>
        /// <param name="exception"> The exception object to log. </param>
        /// <remarks>
        ///     <para> This method first checks if this logger is DEBUG enabled by comparing the level of this logger with the Debug level. If this logger is DEBUG enabled, then it converts the message object (passed as parameter) to a string by invoking the appropriate IObjectRenderer. It then proceeds to call all the registered appenders in this logger and also higher in the hierarchy depending on the value of the additivity flag. </para>
        ///     <para> WARNING Notice that passing an Exception to this method will print the name of the Exception but no stack trace. To print a stack trace use the Debug(object, Exception) form instead. </para>
        /// </remarks>
        [PublicAPI]
        public void Debug(Exception exception) => SafeRunner.Run(this.log.Debug, exception, exception.Message, new object[0]);

        /// <summary>
        ///     Log a message object with the Debug level.
        /// </summary>
        /// <param name="message"> The message object to log </param>
        /// <param name="exception"> The exception object to log. </param>
        /// <remarks>
        /// </remarks>
        [PublicAPI]
        public void Debug(object message, Exception exception) => SafeRunner.Run(this.log.Debug, exception, message.ToString(), new object[0]);

        /// <summary>
        ///     Logs a formatted message string with the Debug level.
        /// </summary>
        /// <param name="format"> A String containing zero or more format items </param>
        /// <param name="args"> An Object array containing zero or more objects to format </param>
        [PublicAPI]
        public void DebugFormatted(string format, params object[] args) => this.DebugFormatted(Thread.CurrentThread.CurrentCulture, format, args);

        /// <summary>
        ///     Logs a formatted message string with the Debug level.
        /// </summary>
        /// <param name="provider"> An object that supplies culture-specific formatting information. </param>
        /// <param name="format"> A String containing zero or more format items </param>
        /// <param name="args"> An Object array containing zero or more objects to format </param>
        [PublicAPI]
        public void DebugFormatted(IFormatProvider provider, string format, params object[] args) => SafeRunner.Run(this.log.Debug, provider, format, args);

        /// <summary>
        ///     Logs a message object with the Info level.
        /// </summary>
        /// <param name="message"> The message object to log. </param>
        [PublicAPI]
        public void Info(object message) => SafeRunner.Run(this.log.Info, message);

        /// <summary>
        ///     Logs a message object with the Info level.
        /// </summary>
        /// <param name="message"> The message object to log. </param>
        /// <param name="exception"> The exception object to log. </param>
        [PublicAPI]
        public void Info(object message, Exception exception) => SafeRunner.Run(this.log.Info, exception, message.ToString(), new object[0]);

        /// <summary>
        ///     Logs a formatted message string with the Info level.
        /// </summary>
        /// <param name="format"> A String containing zero or more format items </param>
        /// <param name="args"> An Object array containing zero or more objects to format </param>
        [PublicAPI]
        public void InfoFormatted(string format, params object[] args) => this.InfoFormatted(Thread.CurrentThread.CurrentCulture, format, args);

        /// <summary>
        ///     Logs a formatted message string with the Info level.
        /// </summary>
        /// <param name="provider"> An object that supplies culture-specific formatting information. </param>
        /// <param name="format"> A String containing zero or more format items </param>
        /// <param name="args"> An Object array containing zero or more objects to format </param>
        [PublicAPI]
        public void InfoFormatted(IFormatProvider provider, string format, params object[] args) => SafeRunner.Run(this.log.Info, provider, format, args);

        /// <summary>
        ///     Log a message object with the Warn level.
        /// </summary>
        /// <param name="message"> The message object to log. </param>
        [PublicAPI]
        public void Warn(object message) => SafeRunner.Run(this.log.Warn, message);

        /// <summary>
        ///     Log a message object with the Warn level including the stack 
        ///     trace of the Exception passed as a parameter.
        /// </summary>
        /// <param name="message"> The message object to log. </param>
        /// <param name="exception"> The exception to log, including its stack trace. </param>
        [PublicAPI]
        public void Warn(object message, Exception exception) => SafeRunner.Run(this.log.Warn, exception, message.ToString(), new object[0]);

        /// <summary>
        ///     Logs a formatted message string with the Warn level.
        /// </summary>
        /// <param name="format"> A String containing zero or more format items </param>
        /// <param name="args"> An Object array containing zero or more objects to format </param>
        [PublicAPI]
        public void WarnFormatted(string format, params object[] args) => this.WarnFormatted(Thread.CurrentThread.CurrentCulture, format, args);

        /// <summary>
        ///     Logs a formatted message string with the Warn level.
        /// </summary>
        /// <param name="provider"> An object that supplies culture-specific formatting information. </param>
        /// <param name="format"> A String containing zero or more format items </param>
        /// <param name="args"> An Object array containing zero or more objects to format </param>
        [PublicAPI]
        public void WarnFormatted(IFormatProvider provider, string format, params object[] args) => SafeRunner.Run(this.log.Warn, provider, format, args);

        /// <summary>
        ///     Logs a message object with the Error level.
        /// </summary>
        /// <param name="message"> The message object to log. </param>
        [PublicAPI]
        public void Error(object message) => SafeRunner.Run(this.log.Error, message);

        /// <summary>
        ///     Log a message object with the Error level including the stack trace of the 
        ///     Exception passed as a parameter.
        /// </summary>
        /// <param name="message"> The message object to log. </param>
        /// <param name="exception"> The exception to log, including its stack trace. </param>
        [PublicAPI]
        public void Error(object message, Exception exception) => SafeRunner.Run(this.log.Error, exception, message.ToString(), new object[0]);

        /// <summary>
        ///     Logs a formatted message string with the Error level.
        /// </summary>
        /// <param name="format"> A String containing zero or more format items </param>
        /// <param name="args"> An Object array containing zero or more objects to format </param>
        [PublicAPI]
        public void ErrorFormatted(string format, params object[] args) => this.ErrorFormatted(Thread.CurrentThread.CurrentCulture, format, args);

        /// <summary>
        ///     Logs a formatted message string with the Error level.
        /// </summary>
        /// <param name="provider"> An object that supplies culture-specific formatting information. </param>
        /// <param name="format"> A String containing zero or more format items </param>
        /// <param name="args"> An Object array containing zero or more objects to format </param>
        [PublicAPI]
        public void ErrorFormatted(IFormatProvider provider, string format, params object[] args) => SafeRunner.Run(this.log.Error, provider, format, args);

        /// <summary>
        ///     Log a message object with the Fatal level.
        /// </summary>
        /// <param name="message"> The message object to log. </param>
        [PublicAPI]
        public void Fatal(object message) => SafeRunner.Run(this.log.Fatal, message);

        /// <summary>
        ///     Log a message object with the Fatal level including the stack trace of 
        ///     the Exception passed as a parameter.
        /// </summary>
        /// <param name="message"> The message object to log. </param>
        /// <param name="exception"> The exception to log, including its stack trace. </param>
        [PublicAPI]
        public void Fatal(object message, Exception exception) => SafeRunner.Run(this.log.Fatal, exception, message.ToString(), new object[0]);

        /// <summary>
        ///     Logs a formatted message string with the Fatal level.
        /// </summary>
        /// <param name="format"> A String containing zero or more format items </param>
        /// <param name="args"> An Object array containing zero or more objects to format </param>
        [PublicAPI]
        public void FatalFormatted(string format, params object[] args) => this.FatalFormatted(Thread.CurrentThread.CurrentCulture, format, args);

        /// <summary>
        ///     Logs a formatted message string with the Fatal level.
        /// </summary>
        /// <param name="provider"> </param>
        /// <param name="format"> A String containing zero or more format items </param>
        /// <param name="args"> An Object array containing zero or more objects to format </param>
        [PublicAPI]
        public void FatalFormatted(IFormatProvider provider, string format, params object[] args) => SafeRunner.Run(this.log.Fatal, provider, format, args);

        /// <summary>
        ///     Gets instance of <see cref="Logger" /> for certain <see cref="Type" />.
        /// </summary>
        /// <param name="loggerName"> The <see cref="Type" /> whose <see cref="Logger" /> name should be retrieved. </param>
        /// <returns> Instance of <see cref="Logger" /> . </returns>
        private static Logger GetLogger(string loggerName)
        {
            if (loggerName == null)
            {
                throw new ArgumentNullException(nameof(loggerName));
            }

            return LogManager.GetLogger(loggerName);
        }
    }
}
