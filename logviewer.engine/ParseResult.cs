// Created by: egr
// Created at: 10.10.2013
// © 2012-2014 Alexander Egorov

namespace logviewer.engine
{
    internal struct ParseResult<T>
    {
        internal bool Result { get; set; }
        internal T Value { get; set; }
    }
}