using System;

namespace JaIoC
{
    public class ObjectFactoryIsAlreadyRegisteredException : IoCException
    {
        public ObjectFactoryIsAlreadyRegisteredException(Type type, object key)
            : base(type, key)
        {
        }
    }
}
