using System;
using System.Collections.Generic;
using System.Text;

namespace System.ComponentModel.Composition
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public class ImportingConstructorAttribute : Attribute
    {
    }
}
