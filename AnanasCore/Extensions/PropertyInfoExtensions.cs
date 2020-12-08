using AnanasCore.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AnanasCore.Extensions
{
    public static class PropertyInfoExtensions
    {
        /// <summary>
        /// Checks if the <paramref name="type"/> has a custom attribute of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool HasCustomAttribute<T>(this PropertyInfo type) where T : Attribute
        {
            return type.GetCustomAttribute<T>() != null;
        }
    }
}
