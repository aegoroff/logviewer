// Created by: egr
// Created at: 31.01.2015
// © 2012-2015 Alexander Egorov

namespace logviewer.engine
{
    internal class StringLiteral : Pattern
    {
        internal StringLiteral(string content) : base(content.UnescapeString())
        {
        }
    }
}