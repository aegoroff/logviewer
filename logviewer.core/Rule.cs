// Created by: egr
// Created at: 13.10.2014
// © 2012-2014 Alexander Egorov

using System;
using System.Diagnostics;

namespace logviewer.core
{
    [DebuggerDisplay("{Pattern}")]
    public struct Rule
    {
        private readonly string pattern;
        private readonly string type;


        public Rule(string type = "string", string pattern = "*")
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
        public string Type
        {
            get { return this.type; }
        }

        public bool Equals(Rule other)
        {
            return string.Equals(this.pattern, other.pattern, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is Rule && this.Equals((Rule)obj);
        }

        public override int GetHashCode()
        {
            return this.pattern != null ? this.pattern.GetHashCode() : 0;
        }

        public static bool operator ==(Rule s1, Rule s2)
        {
            return s1.Equals(s2);
        }

        public static bool operator !=(Rule s1, Rule s2)
        {
            return !(s1 == s2);
        }
    }
}