// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 18.10.2014
// © 2012-2018 Alexander Egorov

using System;
using System.Collections.Generic;

namespace logviewer.engine
{
    /// <summary>
    /// Represents metadata property info
    /// </summary>
    public struct SemanticProperty : IEqualityComparer<SemanticProperty>
    {
        private readonly string name;

        /// <summary>
        /// Initializes new <see cref="SemanticProperty"/> instance using name and parser type specified
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="parser">Data parser info</param>
        public SemanticProperty(string name, ParserType parser)
        {
            this.name = name;
            this.Parser = parser;
        }

        /// <summary>
        /// Implicit creation of the new <see cref="SemanticProperty"/> instance where <see cref="ParserType"/> is string
        /// </summary>
        /// <param name="name">Property name</param>
        /// <returns>new <see cref="SemanticProperty"/> instance</returns>
        public static implicit operator SemanticProperty(string name) => new SemanticProperty(name, ParserType.String);

        /// <summary>
        /// Gets property name
        /// </summary>
        public string Name => this.name;

        /// <summary>
        /// Gets property parser type
        /// </summary>
        public ParserType Parser { get; }

        /// <summary>
        /// Compares this instance with instance specified
        /// </summary>
        /// <param name="other">Instance to compare with</param>
        /// <returns>True if others' instance property equals to this property</returns>
        public bool Equals(SemanticProperty other) => string.Equals(this.name, other.name, StringComparison.OrdinalIgnoreCase);

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

            return obj is SemanticProperty property && this.Equals(property);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode() => this.name?.GetHashCode() ?? 0;

        /// <summary>
        /// Compare two instance for equality
        /// </summary>
        /// <param name="s1">First instance</param>
        /// <param name="s2">Second instance</param>
        /// <returns>True if instances are equivalent false otherwise</returns>
        public static bool operator ==(SemanticProperty s1, SemanticProperty s2) => s1.Equals(s2);

        /// <summary>
        /// Compare two instance for difference
        /// </summary>
        /// <param name="s1">First instance</param>
        /// <param name="s2">Second instance</param>
        /// <returns>True if instances are not equivalent false otherwise</returns>
        public static bool operator !=(SemanticProperty s1, SemanticProperty s2) => !(s1 == s2);

        /// <inheritdoc />
        public bool Equals(SemanticProperty x, SemanticProperty y) => x == y;

        /// <inheritdoc />
        public int GetHashCode(SemanticProperty obj) => obj.GetHashCode();
    }
}
