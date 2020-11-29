using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class NonPersistentAttribute : Attribute
    {
    }
}
