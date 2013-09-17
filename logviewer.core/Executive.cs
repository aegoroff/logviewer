// Created by: egr
// Created at: 19.09.2012
// © 2012-2013 Alexander Egorov

using System;

namespace logviewer.core
{
    /// <summary>
    ///     Provides useful excecutive primitives.
    /// </summary>
    public static class Executive
    {
        /// <summary>
        ///     Runs specified method and handles all errors. Error messaged are written into log.
        /// </summary>
        /// <param name="method"> Method to run </param>
        public static void SafeRun(Action method)
        {
            try
            {
                method();
            }
            catch (Exception e)
            {
                Log.Instance.Error(e.Message, e);
            }
        }

        /// <summary>
        ///     Runs specified method and handles all errors. Error messaged are written into log.
        /// </summary>
        /// <param name="method"> Method to run </param>
        public static T SafeRun<T>(Func<T> method)
        {
            try
            {
                return method();
            }
            catch (Exception e)
            {
                Log.Instance.Error(e.Message, e);
            }
            return default(T);
        }

        /// <summary>
        ///     Runs specified method and handles all errors. Error messaged are written into log.
        /// </summary>
        /// <typeparam name="T"> Argument type </typeparam>
        /// <param name="method"> Method to run </param>
        /// <param name="argument"> Argument value </param>
        public static void SafeRun<T>(Action<T> method, T argument)
        {
            try
            {
                method(argument);
            }
            catch (Exception e)
            {
                Log.Instance.Error(e.Message, e);
            }
        }

        /// <summary>
        ///     Runs specified method and handles all errors. Error messaged are written into log.
        /// </summary>
        /// <typeparam name="TReturn"> Return type </typeparam>
        /// <typeparam name="TArg"> Argument type </typeparam>
        /// <param name="method"> Method to run </param>
        /// <param name="argument"> Argument value </param>
        public static TReturn SafeRun<TReturn, TArg>(Func<TArg, TReturn> method, TArg argument)
        {
            try
            {
                return method(argument);
            }
            catch (Exception e)
            {
                Log.Instance.Error(e.Message, e);
            }
            return default(TReturn);
        }
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