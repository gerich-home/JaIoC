using System;
using System.Reflection;

namespace JaIoC
{
    public class ParameterNotResolvedException : IoCException
    {
        public ParameterNotResolvedException(ConstructorInfo constructor, int parameterIndex, IoCException innerException, Type type, object key)
            : base(string.Format("Can't resolve parameter {0}: {1} of type {2} in constructor of {3}",
                                 parameterIndex,
                                 constructor.GetParameters()[parameterIndex].Name,
                                 constructor.GetParameters()[parameterIndex].ParameterType,
                                 type),
                    innerException, type, key)
        {
        }
    }
}