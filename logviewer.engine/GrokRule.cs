﻿// Created by: egr
// Created at: 13.10.2014
// © 2012-2015 Alexander Egorov

using System;
using System.Diagnostics;

namespace logviewer.engine
{
    
    /// <summary>
    /// Represents metadata rule
    /// </summary>
    [DebuggerDisplay("{Pattern}")]
    public struct GrokRule
    {
        private readonly string pattern;
        private readonly ParserType type;

        /// <summary>
        /// Initializes new <see cref="GrokRule"/> instance
        /// </summary>
        /// <param name="type">Data type</param>
        /// <param name="pattern">Matching pattern</param>
        public GrokRule(ParserType type = ParserType.String, string pattern = "*")
        {
            this.pattern = pattern;
            this.type = type;
        }

        /// <summary>
        /// Gets or sets data pattern that will be cast to type
        /// </summary>
        public string Pattern
        {
            get { return this.pattern; }
        }

        /// <summary>
        /// Gets or sets target casting type
        /// </summary>
        public ParserType Type
        {
            get { return this.type; }
        }

        /// <summary>
        /// Compares this instance with instance specified
        /// </summary>
        /// <param name="other">Intance to compare with</param>
        /// <returns>True if other's intance pattern equals to this pattern</returns>
        public bool Equals(GrokRule other)
        {
            return string.Equals(this.pattern, other.pattern, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is GrokRule && this.Equals((GrokRule)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return this.pattern.Return(s => s.GetHashCode(), 0);
        }

        /// <summary>
        /// Compare two instance for equality
        /// </summary>
        /// <param name="r1">First instance</param>
        /// <param name="r2">Second instance</param>
        /// <returns>True if instances are equivalent false otherwise</returns>
        public static bool operator ==(GrokRule r1, GrokRule r2)
        {
            return r1.Equals(r2);
        }

        /// <summary>
        /// Compare two instance for difference
        /// </summary>
        /// <param name="r1">First instance</param>
        /// <param name="r2">Second instance</param>
        /// <returns>True if instances are not equivalent false otherwise</returns>
        public static bool operator !=(GrokRule r1, GrokRule r2)
        {
            return !(r1 == r2);
        }
    }
}