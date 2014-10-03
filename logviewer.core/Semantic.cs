// Created by: egr
// Created at: 03.10.2014
// © 2012-2014 Alexander Egorov

using System;
using System.Diagnostics;

namespace logviewer.core
{
    [DebuggerDisplay("{Name}")]
    public struct Semantic
    {
        private readonly string name;
        private string type;

        public Semantic(string name, string type = "string")
        {
            this.name = name;
            this.type = type;
        }

        public string Type
        {
            get { return this.type; }
            set { this.type = value; }
        }

        public string Name
        {
            get { return this.name; }
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