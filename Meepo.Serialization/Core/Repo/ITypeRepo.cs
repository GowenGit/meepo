using System;

namespace Meepo.Serialization.Core.Repo
{
    internal interface ITypeRepo
    {
        Type GetType(int packageType);
    }
}