// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 18.10.2014
// © 2012-2017 Alexander Egorov

using System;

namespace logviewer.engine
{
    /// <summary>
    /// Represents all possible metadata types
    /// </summary>
    public enum ParserType
    {
        /// <summary>
        /// <see cref="LogLevel"/> type
        /// </summary>
        LogLevel,
        
        /// <summary>
        /// <see cref="DateTime"/> type
        /// </summary>
        Datetime,
        
        /// <summary>
        /// Integer (<see cref="long"/>) type
        /// </summary>
        Interger,
        
        /// <summary>
        /// String type
        /// </summary>
        String
    }
}