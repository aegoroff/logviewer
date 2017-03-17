// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 04.12.2015
// © 2007-2016 Alexander Egorov

using System.Data;
using System.Data.SQLite;
using System.Runtime.CompilerServices;

namespace logviewer.logic.storage
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
            cmd.Parameters.Add(new SQLiteParameter
            {
                ParameterName = name,
                Value = value
            });
        }
    }
}