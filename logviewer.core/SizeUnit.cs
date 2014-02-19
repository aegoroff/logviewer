// Created by: egr
// Created at: 19.09.2012
// © 2012-2014 Alexander Egorov

namespace logviewer.core
{
    /// <summary>
    /// Represents size unit tag bytes, Kbytes, Mbytes etc.
    /// </summary>
    public enum SizeUnit
    {
        Bytes = 0,
        KBytes = 1,
        MBytes = 2,
        GBytes = 3,
        TBytes = 4,
        PBytes = 5,
        EBytes = 6,
        ZBytes = 7,
        YBytes = 8,
        BBytes = 9,
        GPBytes = 10
    }
}