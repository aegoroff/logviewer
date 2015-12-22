// Created by: egr
// Created at: 10.01.2014
// © 2012-2015 Alexander Egorov

using System;

namespace logviewer.logic.storage
{
    [AttributeUsage(AttributeTargets.Property)]
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