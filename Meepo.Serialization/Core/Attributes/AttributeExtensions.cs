using System;
using System.Reflection;
using Meepo.Serialization.Core.Exceptions;

namespace Meepo.Serialization.Core.Attributes
{
    internal static class AttributeExtensions
    {
        public static int GetMeepoPackageAttributeValue<T>(this T package)
        {
            var attribute = typeof(T).GetMeepoPackageAttribute();

            if (attribute == null) throw new MeepoSerializationException("No meepo package attribute for this type was found!");

            return attribute.PackageType;
        }

        public static MeepoPackageAttribute GetMeepoPackageAttribute(this Type type)
        {
            return type.GetTypeInfo().GetCustomAttribute<MeepoPackageAttribute>();
        }
    }
}
