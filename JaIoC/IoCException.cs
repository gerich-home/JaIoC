using System;

namespace JaIoC
{
    public class IoCException : Exception
    {
        public Type Type { get; private set; }
        public object Key { get; private set; }

        public IoCException(Type type, object key)
            : base(string.Format("Type: {0}{1}", type, key == null ? "" : string.Format(", Key: {0}", key)))
        {
            Type = type;
            Key = key;
        }

        public IoCException(string message, Type type, object key)
            : base(message)
        {
            Type = type;
            Key = key;
        }

        public IoCException(string message, IoCException innerException, Type type, object key)
            : base(message, innerException)
        {
            Type = type;
            Key = key;
        }
    }
}