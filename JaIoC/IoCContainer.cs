using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JaIoC;

namespace JaIoC
{
    public class IoCContainer : IIoCContainer
    {
        
        private readonly IDictionary<IoCEntryKey, Func<IIoCSession, object>> _factories;
        private readonly List<IoCEntryKey> _used = new List<IoCEntryKey>();

        private class IoCSession : IIoCSession
        {
            private readonly IoCContainer _ioc;
            private readonly Stack<IoCEntryKey> _currentlyExecutedFactories = new Stack<IoCEntryKey>();

            public IoCSession(IoCContainer ioc)
            {
                _ioc = ioc;
            }

            public Func<T> FactoryFor<T>(object key = null)
                where T : class
            {
                Func<IIoCSession, object> factory;

                if (_ioc._factories.TryGetValue(new IoCEntryKey(typeof(T), key), out factory))
                {
                    return () => (factory as Func<IIoCSession, T>)(this);
                }

                IoCEntryKey matchedEntryKey = null;

                foreach (var matchingEntryKey in _ioc._factories.Keys.Where(entryKey => (entryKey.Key == key || (entryKey.Key != null && entryKey.Key.Equals(key))) && typeof(T).IsAssignableFrom(entryKey.Type)))
                {
                    if (matchedEntryKey != null)
                        throw new ObjectFactoryIsAmbiguosException(typeof(T), key);

                    matchedEntryKey = matchingEntryKey;
                }

                if (matchedEntryKey == null)
                    throw new ObjectFactoryIsNotRegisteredException(typeof(T), key);

                return () =>
                    {
                        if (!_ioc._used.Contains(matchedEntryKey))
                            _ioc._used.Add(matchedEntryKey);

                        if (_currentlyExecutedFactories.Contains(matchedEntryKey))
                            throw new InvalidOperationException(string.Format("Factory for {0} is already being executed", matchedEntryKey.Type));

                        _currentlyExecutedFactories.Push(matchedEntryKey);

                        var executingFactory = _ioc._factories[matchedEntryKey] as Func<IIoCSession, T>;
                        var result = executingFactory(this);

                        _currentlyExecutedFactories.Pop();

                        return result;
                    };
            }
        }

        public IoCContainer(IDictionary<IoCEntryKey, Func<IIoCSession, object>> factories)
        {
            _factories = factories;
        }

        public IIoCSession Start()
        {
            return new IoCSession(this);
        }

        public void DumpUnusedEntries()
        {
            foreach (var key in _factories.Keys.Except(_used))
            {
                Debug.WriteLine("Key {0} was never used", key);
            }
        }
    }
}
