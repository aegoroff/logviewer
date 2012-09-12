using System;
using System.Collections.Generic;

namespace logviewer
{
    /// <summary>
    ///     Represents method that accept and return nothing
    /// </summary>
    public delegate void VoidMethod();

    /// <summary>
    ///     Represents method that accept nothing and return a <typeparamref name="T" /> instance
    /// </summary>
    /// <typeparam name="T"> Method return type </typeparam>
    /// <returns> </returns>
    public delegate T ParameterlessMethod<out T>();

    /// <summary>
    ///     Represents method that accept an argument of type<typeparamref name="T" /> and return nothing.
    /// </summary>
    /// <typeparam name="T"> Argument type </typeparam>
    /// <param name="argument"> Argument value </param>
    public delegate void OneArgumentMethod<in T>(T argument);

    /// <summary>
    ///     Represents method that accept an argument of type<typeparamref name="TArg" /> and return a <typeparamref name="TReturn" /> instance.
    /// </summary>
    /// <typeparam name="TReturn"> Return type </typeparam>
    /// <typeparam name="TArg"> Argument type </typeparam>
    /// <param name="argument"> Argument value </param>
    /// <returns> </returns>
    public delegate TReturn OneArgumentReturningMethod<out TReturn, in TArg>(TArg argument);

    /// <summary>
    ///     Provides useful excecutive primitives.
    /// </summary>
    public static class Executive
    {
        private static readonly IDictionary<LogLevel, LoggingMethod> methods = new Dictionary<LogLevel, LoggingMethod>
            {
                { LogLevel.Trace, Log.Instance.Trace },
                { LogLevel.Debug, Log.Instance.Debug },
                { LogLevel.Info, Log.Instance.Info },
                { LogLevel.Warn, Log.Instance.Warn },
                { LogLevel.Error, Log.Instance.Error },
                { LogLevel.Fatal, Log.Instance.Fatal },
            };

        /// <summary>
        ///     Runs specified method and handles all errors. Error messaged are written into log.
        /// </summary>
        /// <param name="method"> Method to run </param>
        /// <param name="level"> Logging level </param>
        public static void SafeRun(VoidMethod method, LogLevel level = LogLevel.Error)
        {
            try
            {
                method();
            }
            catch (Exception e)
            {
                methods[level](e.Message, e);
            }
        }

        /// <summary>
        ///     Runs specified method and handles all errors. Error messaged are written into log.
        /// </summary>
        /// <param name="method"> Method to run </param>
        /// <param name="level"> Logging level </param>
        public static T SafeRun<T>(ParameterlessMethod<T> method, LogLevel level = LogLevel.Error)
        {
            try
            {
                return method();
            }
            catch (Exception e)
            {
                methods[level](e.Message, e);
            }
            return default(T);
        }

        /// <summary>
        ///     Runs specified method and handles all errors. Error messaged are written into log.
        /// </summary>
        /// <typeparam name="T"> Argument type </typeparam>
        /// <param name="method"> Method to run </param>
        /// <param name="argument"> Argument value </param>
        /// <param name="level"> Logging level </param>
        public static void SafeRun<T>(OneArgumentMethod<T> method, T argument, LogLevel level = LogLevel.Error)
        {
            try
            {
                method(argument);
            }
            catch (Exception e)
            {
                methods[level](e.Message, e);
            }
        }

        /// <summary>
        ///     Runs specified method and handles all errors. Error messaged are written into log.
        /// </summary>
        /// <typeparam name="TReturn"> Return type </typeparam>
        /// <typeparam name="TArg"> Argument type </typeparam>
        /// <param name="method"> Method to run </param>
        /// <param name="argument"> Argument value </param>
        /// <param name="level"> Logging level </param>
        public static TReturn SafeRun<TReturn, TArg>(OneArgumentReturningMethod<TReturn, TArg> method, TArg argument, LogLevel level = LogLevel.Error)
        {
            try
            {
                return method(argument);
            }
            catch (Exception e)
            {
                methods[level](e.Message, e);
            }
            return default(TReturn);
        }

        #region Nested type: LoggingMethod

        private delegate void LoggingMethod(object message, Exception exception);

        #endregion
    }

    /// <summary>
    ///     Logging Level
    /// </summary>
    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }
}