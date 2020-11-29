using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ManyToManyAssociationAttribute : Attribute
    {
        public string Name { get; }
        public string AssociatedName { get; }

        public ManyToManyAssociationAttribute(string name = "", string associatedName = "")
        {
            Name = name;
            AssociatedName = associatedName;
        }
    }
}
