// Created by: egr
// Created at: 03.10.2014
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace logviewer.core
{
    [DebuggerDisplay("{Name}")]
    public struct Semantic
    {
        private readonly string name;
        private readonly HashSet<Rule> castingRules;

        public Semantic(string name)
        {
            this.name = name;
            this.castingRules = new HashSet<Rule>();
        }
        
        public Semantic(string name, Rule rule) : this(name)
        {
            this.castingRules.Add(rule);
        }

        public string Name
        {
            get { return this.name; }
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
            return string.Equals(this.name, other.name, StringComparison.OrdinalIgnoreCase);
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
            return this.name != null ? this.name.GetHashCode() : 0;
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