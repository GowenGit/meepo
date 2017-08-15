using System;

namespace Meepo.Serialization.Core.Attributes
{
    public class MeepoPackageAttribute : Attribute
    {
        public int PackageType { get; }

        public MeepoPackageAttribute(int packageType)
        {
            PackageType = packageType;
        }
    }
}
