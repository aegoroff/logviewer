// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 19.09.2012
// © 2012-2017 Alexander Egorov

namespace logviewer.engine
{
    /// <summary>
    /// Represents size unit tag bytes, Kbytes, Mbytes etc.
    /// </summary>
    public enum SizeUnit
    {
        /// <summary>
        /// Size in bytes
        /// </summary>
        Bytes = 0,
        
        /// <summary>
        /// Size in kilobytes
        /// </summary>
        KBytes = 1,
        
        /// <summary>
        /// Size in megabytes
        /// </summary>
        MBytes = 2,
        
        /// <summary>
        /// Size in gigabytes
        /// </summary>
        GBytes = 3,
        
        /// <summary>
        /// Size in terrabytes
        /// </summary>
        TBytes = 4,
        
        /// <summary>
        /// Size in petabytes
        /// </summary>
        PBytes = 5,
        
        /// <summary>
        /// Size in exabytes
        /// </summary>
        EBytes = 6,
        
        /// <summary>
        /// Size un zettabytes
        /// </summary>
        ZBytes = 7,
        
        /// <summary>
        /// Size in yottabytes
        /// </summary>
        YBytes = 8,
        
        /// <summary>
        /// Size in brontobytes
        /// </summary>
        BBytes = 9,
        
        /// <summary>
        /// Size in geopbytes
        /// </summary>
        GPBytes = 10
    }
}