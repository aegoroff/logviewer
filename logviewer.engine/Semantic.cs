// Created by: egr
// Created at: 03.10.2014
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace logviewer.engine
{
    [DebuggerDisplay("{Property}")]
    public struct Semantic
    {
        private readonly string property;
        private readonly HashSet<Rule> castingRules;

        public Semantic(string property)
        {
            this.property = property;
            this.castingRules = new HashSet<Rule>();
        }
        
        public Semantic(string property, Rule rule) : this(property)
        {
            this.castingRules.Add(rule);
        }

        public string Property
        {
            get { return this.property; }
        }

        public ISet<Rule> CastingRules
        {
            get { return this.castingRules; }
        }

        public bool Contains(Rule rule)
        {
            return this.castingRules.Contains(rule);
        }

        public bool Equals(Semantic other)
        {
            return string.Equals(this.property, other.property, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is Semantic && this.Equals((Semantic)obj);
        }

        public override int GetHashCode()
        {
            return this.property != null ? this.property.GetHashCode() : 0;
        }

        public static bool operator ==(Semantic s1, Semantic s2)
        {
            return s1.Equals(s2);
        }

        public static bool operator !=(Semantic s1, Semantic s2)
        {
            return !(s1 == s2);
        }
    }
}