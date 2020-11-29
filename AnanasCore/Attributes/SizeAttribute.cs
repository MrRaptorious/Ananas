using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SizeAttribute : Attribute
    {
        public int Length { get; }

        public SizeAttribute(int length = 255)
        {
            Length = length;
        }

    }
}
