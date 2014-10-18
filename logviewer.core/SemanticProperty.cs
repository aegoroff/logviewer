// Created by: egr
// Created at: 18.10.2014
// © 2012-2014 Alexander Egorov

using System;

namespace logviewer.core
{
    public struct SemanticProperty
    {
        private readonly string name;
        private readonly ParserType parser;

        public SemanticProperty(string name, ParserType parser)
        {
            this.name = name;
            this.parser = parser;
        }

        public static implicit operator SemanticProperty(string name)
        {
            return new SemanticProperty(name, ParserType.String);
        }

        /// <summary>
        /// Gets property name
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets property parser type
        /// </summary>
        public ParserType Parser
        {
            get { return this.parser; }
        }

        public bool Equals(SemanticProperty other)
        {
            return string.Equals(this.name, other.name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is SemanticProperty && this.Equals((SemanticProperty)obj);
        }

        public override int GetHashCode()
        {
            return this.name != null ? this.name.GetHashCode() : 0;
        }

        public static bool operator ==(SemanticProperty s1, SemanticProperty s2)
        {
            return s1.Equals(s2);
        }

        public static bool operator !=(SemanticProperty s1, SemanticProperty s2)
        {
            return !(s1 == s2);
        }
    }
}