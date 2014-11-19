// Created by: egr
// Created at: 10.10.2013
// © 2012-2014 Alexander Egorov

namespace logviewer.engine
{
    public struct ParseResult<T>
    {
        public bool Result { get; set; }
        public T Value { get; set; }
    }
}