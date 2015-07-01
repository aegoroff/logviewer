// Created by: egr
// Created at: 25.10.2012
// © 2012-2015 Alexander Egorov

namespace logviewer.engine
{
    /// <summary>
    ///     Logging Level
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// No level detected
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