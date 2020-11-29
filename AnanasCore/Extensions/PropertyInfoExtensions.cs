using AnanasCore.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AnanasCore.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static bool HasCustomAttribute<T>(this PropertyInfo type)
        {
            return type.GetCustomAttribute<SizeAttribute>() != null;
        }
    }
}
