// Created by: egr
// Created at: 10.01.2014
// © 2012-2014 Alexander Egorov

using System;

namespace logviewer.core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
        
        public bool Nullable { get; set; }
    }
}