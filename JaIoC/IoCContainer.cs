using System;
using System.Collections.Generic;

namespace JaIoC
{
    public class IoCContainer : IIoCContainer
    {
        private readonly IDictionary<IoCEntryKey, Func<IIoCSession, object>> _factories;

        public IoCContainer(IDictionary<IoCEntryKey, Func<IIoCSession, object>> factories)
        {
            _factories = factories;
        }

        public IIoCSession Start()
        {
            return new IoCSession(_factories);
        }
    }
}
