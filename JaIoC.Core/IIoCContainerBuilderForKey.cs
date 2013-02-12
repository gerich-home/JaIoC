using System;

namespace JaIoC
{
    public interface IIoCContainerBuilderForKey
        : IFluentInterface
    {
        object Key { get; }

        IIoCContainerBuilder Register<T>(Func<IIoCSession, T> factory)
            where T : class;
    }
}