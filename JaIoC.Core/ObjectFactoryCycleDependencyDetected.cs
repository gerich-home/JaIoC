using System;

namespace JaIoC
{
    public class ObjectFactoryCycleDependencyDetected : IoCException
    {
        public ObjectFactoryCycleDependencyDetected(Type type, object key)
            : base(type, key)
        {
        }
    }
}