// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 03.10.2014
// © 2012-2018 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace logviewer.engine
{
    /// <summary>
    /// Represent all possible transformations for a named property
    /// </summary>
    [DebuggerDisplay("{" + nameof(Property) + "}")]
    public struct Semantic : IEquatable<Semantic>
    {
        private readonly string property;

        private readonly HashSet<GrokRule> castingRules;

        /// <summary>
        /// Initializes new <see cref="Semantic"/> instance using name specified.
        /// </summary>
        /// <param name="property">Property name</param>
        public Semantic(string property)
        {
            this.property = property;
            this.castingRules = new HashSet<GrokRule>();
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes new <see cref="T:logviewer.engine.Semantic" /> instance using name and rule specified.
        /// </summary>
        /// <param name="property">Property name</param>
        /// <param name="rule">Casting rule</param>
        public Semantic(string property, GrokRule rule) : this(property) => this.castingRules.Add(rule);

        /// <summary>
        /// Gets semantic property name
        /// </summary>
        public string Property => this.property;

        /// <summary>
        /// Gets all possible casting rules
        /// </summary>
        public ISet<GrokRule> CastingRules => this.castingRules;

        /// <summary>
        /// Gets whether this container has <see cref="GrokRule"/> specified
        /// </summary>
        /// <param name="rule">Rule to validate</param>
        /// <returns>True if this container has desired casting rule. False otherwise</returns>
        public bool Contains(GrokRule rule)
        {
            return this.castingRules.Contains(rule);
        }

        /// <summary>
        /// Compares this instance with instance specified
        /// </summary>
        /// <param name="other">Instance to compare with</param>
        /// <returns>True if others' instance property equals to this property</returns>
        public bool Equals(Semantic other) => string.Equals(this.property, other.property, StringComparison.OrdinalIgnoreCase);

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

            return obj is Semantic semantic && this.Equals(semantic);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode() => this.property?.GetHashCode() ?? 0;

        /// <summary>
        /// Compare two instance for equality
        /// </summary>
        /// <param name="s1">First instance</param>
        /// <param name="s2">Second instance</param>
        /// <returns>True if instances are equivalent false otherwise</returns>
        public static bool operator ==(Semantic s1, Semantic s2) => s1.Equals(s2);

        /// <summary>
        /// Compare two instance for difference
        /// </summary>
        /// <param name="s1">First instance</param>
        /// <param name="s2">Second instance</param>
        /// <returns>True if instances are not equivalent false otherwise</returns>
        public static bool operator !=(Semantic s1, Semantic s2) => !(s1 == s2);
    }
}
