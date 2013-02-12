using System;

namespace JaIoC
{
    public class ObjectFactoryIsAmbiguosException : IoCException
    {
        public ObjectFactoryIsAmbiguosException(Type type, object key)
            : base(type, key)
        {
        }
    }
}