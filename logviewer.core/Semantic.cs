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
        private readonly IDictionary<string, string> castingRules; 

        public Semantic(string name, string pattern = null, string type = null)
        {
            this.name = name;
            this.castingRules = new Dictionary<string, string>();
            if (pattern != null && type != null)
            {
                this.castingRules.Add(pattern, type);
            }
        }

        public string Name
        {
            get { return this.name; }
        }

        public IDictionary<string, string> CastingRules
        {
            get { return this.castingRules; }
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