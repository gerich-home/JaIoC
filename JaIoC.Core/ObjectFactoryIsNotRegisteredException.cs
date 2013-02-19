using System;

namespace JaIoC
{
    public class ObjectFactoryIsNotRegisteredException : IoCException
    {
        public ObjectFactoryIsNotRegisteredException(Type type, object key)
            : base(type, key)
        {
        }
    }
}