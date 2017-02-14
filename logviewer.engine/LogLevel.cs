// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 25.10.2012
// © 2012-2016 Alexander Egorov

namespace logviewer.engine
{
    /// <summary>
    ///     Logging Level
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// -1 No level detected
        /// </summary>
        None = -1,
        
        /// <summary>
        /// 0 - trace
        /// </summary>
        Trace = 0,
        
        /// <summary>
        /// 1 - debug
        /// </summary>
        Debug = 1,
        
        /// <summary>
        /// 2 - info
        /// </summary>
        Info = 2,
        
        /// <summary>
        /// 3 - warning
        /// </summary>
        Warn = 3,
        
        /// <summary>
        /// 4 - error
        /// </summary>
        Error = 4,
        
        /// <summary>
        /// 5 - fatal (critical error)
        /// </summary>
        Fatal = 5
    }
}