using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AssociationAttribute : Attribute
    {
        public string Name { get; }
        public AssociationAttribute(string name = null)
        {
            Name = name;
        }
    }
}
