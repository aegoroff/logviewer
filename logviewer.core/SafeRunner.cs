using System;

namespace logviewer.core
{
    /// <summary>
    ///     Represents method that accept an argument of type<typeparamref name="T" /> and return nothing.
    /// </summary>
    /// <typeparam name="T"> Argument type </typeparam>
    /// <param name="argument"> Argument value </param>
    internal delegate void ArgumentMethod<in T>(T argument);

    /// <summary>
    ///     Represents method that accept arguments of type <typeparamref name="T1" /> and <typeparamref name="T2" /> and return nothing.
    /// </summary>
    /// <typeparam name="T1"> Argument 1 type </typeparam>
    /// <typeparam name="T2"> Argument 2 type </typeparam>
    /// <param name="argument1"> Argument 1 value </param>
    /// <param name="argument2"> Argument 2 value </param>
    internal delegate void ArgumentMethod<in T1, in T2>(T1 argument1, T2 argument2);

    /// <summary>
    ///     Represents method that accept arguments of type <typeparamref name="T1" />, <typeparamref name="T2" /> and <typeparamref
    ///      name="T3" /> and return nothing.
    /// </summary>
    /// <typeparam name="T1"> Argument 1 type </typeparam>
    /// <typeparam name="T2"> Argument 2 type </typeparam>
    /// <typeparam name="T3"> Argument 3 type </typeparam>
    /// <param name="argument1"> Argument 1 value </param>
    /// <param name="argument2"> Argument 2 value </param>
    /// <param name="argument3"> Argument 3 value </param>
    internal delegate void ArgumentMethod<in T1, in T2, in T3>(T1 argument1, T2 argument2, T3 argument3);

    /// <summary>
    ///     Provides useful safe runner primitives.
    /// </summary>
    internal static class SafeRunner
    {
        /// <summary>
        ///     Runs specified method and handles all errors. Error messaged are written into log.
        /// </summary>
        /// <param name="method"> Method to run </param>
        internal static T Run<T>(ParameterlessMethod<T> method)
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
        internal static void Run<T>(ArgumentMethod<T> method, T argument)
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
        internal static void Run<T1, T2>(ArgumentMethod<T1, T2> method, T1 argument1, T2 argument2)
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
        internal static void Run<T1, T2, T3>(ArgumentMethod<T1, T2, T3> method, T1 argument1, T2 argument2, T3 argument3)
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