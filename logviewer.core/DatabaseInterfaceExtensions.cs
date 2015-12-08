// Created by: egr
// Created at: 04.12.2015
// © 2007-2015 Alexander Egorov

using System.Data;
using System.Data.SQLite;
using System.Runtime.CompilerServices;

namespace logviewer.core
{
    internal static class DatabaseInterfaceExtensions
    {
        /// <summary>
        /// Adds named parameter name/parameter value pair into command
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddParameter<T>(this IDbCommand cmd, string name, T value)
        {
            var parameter = SQLiteFactory.Instance.CreateParameter();
            if (parameter == null)
            {
                return;
            }
            parameter.ParameterName = name;
            parameter.Value = value;
            cmd.Parameters.Add(parameter);
        }
    }
}