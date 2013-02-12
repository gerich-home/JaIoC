using System;
using System.Collections.Generic;
using System.Reflection;

namespace JaIoC
{
    public class NoAppropriateContructorException : IoCException
    {
        public NoAppropriateContructorException(Type type, object key, Dictionary<ConstructorInfo, IoCException> caughtExceptions)
            : base(string.Format("Can't find any appropriate constructor for {0}", type), type, key)
        {
        }
    }
}