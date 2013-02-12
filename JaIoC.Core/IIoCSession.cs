using System;

namespace JaIoC
{
    public interface IIoCSession
    {
        Func<T> FactoryFor<T>(object key = null)
            where T : class;
    }
}