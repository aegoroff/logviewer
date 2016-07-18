// Created by: egr
// Created at: 21.09.2012
// © 2012-2016 Alexander Egorov

using System;
using System.Runtime.CompilerServices;

using logviewer.logic.Annotations;

namespace logviewer.logic.support
{
    /// <summary>
    ///     Provides useful safe runner primitives.
    /// </summary>
    public static class SafeRunner
    {
        /// <summary>
        ///     Runs specified method and handles all errors. Error messaged are written into log.
        /// </summary>
        /// <param name="method"> Method to run </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PublicAPI]
        public static void Run(Action method)
        {
            try
            {
                method();
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
            }
        }
        
        /// <summary>
        ///     Runs specified method and handles all errors. Error messaged are written into log.
        /// </summary>
        /// <param name="method"> Method to run </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PublicAPI]
        public static T Run<T>(Func<T> method)
        {
            try
            {
                return method();
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
            }
            return default(T);
        }

        /// <summary>
        ///     Runs specified method and handles all errors. Error messaged are written into log.
        /// </summary>
        /// <typeparam name="T"> Argument type </typeparam>
        /// <param name="method"> Method to run </param>
        /// <param name="argument"> Argument value </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PublicAPI]
        public static void Run<T>(Action<T> method, T argument)
        {
            try
            {
                method(argument);
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
            }
        }

        /// <summary>
        ///     Runs specified method and handles all errors. Error messaged are written into log.
        /// </summary>
        /// <typeparam name="T1"> Argument 1 type </typeparam>
        /// <typeparam name="T2"> Argument 2 type </typeparam>
        /// <param name="method"> Method to run </param>
        /// <param name="argument1"> Argument 1 value </param>
        /// <param name="argument2"> Argument 2 value </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PublicAPI]
        public static void Run<T1, T2>(Action<T1, T2> method, T1 argument1, T2 argument2)
        {
            try
            {
                method(argument1, argument2);
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
            }
        }

        /// <summary>
        ///     Runs specified method and handles all errors. Error messaged are written into log.
        /// </summary>
        /// <typeparam name="T1"> Argument 1 type </typeparam>
        /// <typeparam name="T2"> Argument 2 type </typeparam>
        /// <typeparam name="T3"> Argument 2 type </typeparam>
        /// <param name="method"> Method to run </param>
        /// <param name="argument1"> Argument 1 value </param>
        /// <param name="argument2"> Argument 2 value </param>
        /// <param name="argument3"> Argument 3 value </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PublicAPI]
        public static void Run<T1, T2, T3>(Action<T1, T2, T3> method, T1 argument1, T2 argument2, T3 argument3)
        {
            try
            {
                method(argument1, argument2, argument3);
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
            }
        }
    }
}