﻿namespace System.ComponentModel.Composition
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ExportAttribute : Attribute
    {
        public ExportAttribute()
        {

        }

        public ExportAttribute(Type type)
        {

        }
    }
}
