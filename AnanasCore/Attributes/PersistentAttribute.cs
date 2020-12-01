using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class PersistentAttribute : Attribute
    {
        public string Name { get; set; }
        public PersistentAttribute(string name = "")
        {
            Name = name;
        }
    }
}
