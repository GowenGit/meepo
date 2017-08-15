using System;
using System.Collections.Generic;
using System.Reflection;
using Meepo.Serialization.Core.Attributes;
using Meepo.Serialization.Core.Exceptions;

namespace Meepo.Serialization.Core.Repo
{
    internal class TypeRepo : ITypeRepo
    {
        private readonly Dictionary<int, Type> mappedTypes = new Dictionary<int, Type>();

        public TypeRepo(IEnumerable<Assembly> assemblies)
        {
            MapTypes(assemblies);
        }

        private void MapTypes(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attribute = type.GetMeepoPackageAttribute();

                    if (attribute == null) continue;

                    mappedTypes[attribute.PackageType] = type;
                }
            }
        }

        public Type GetType(int packageType)
        {
            if (!mappedTypes.ContainsKey(packageType)) throw new MeepoSerializationException($"No type was found for package type: {packageType}!");

            return mappedTypes[packageType];
        }
    }
}
