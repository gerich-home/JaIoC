using System;
using System.Collections.Generic;
using System.Linq;

namespace JaIoC
{
    public class IoCSession : IIoCSession
    {
        private readonly IDictionary<IoCEntryKey, Func<IIoCSession, object>> _factories;
        private readonly IList<IoCEntryKey> _currentlyExecutedFactories = new List<IoCEntryKey>();

        public IoCSession(IDictionary<IoCEntryKey, Func<IIoCSession, object>> factories)
        {
            _factories = factories;
        }

        public Func<T> FactoryFor<T>(object key = null)
            where T : class
        {
            Func<IIoCSession, object> factory;

            var matchedEntryKey = new IoCEntryKey(typeof(T), key);

            if(!_factories.TryGetValue(matchedEntryKey, out factory))
            {
                matchedEntryKey = null;
                foreach (var matchingEntryKey in _factories.Keys.Where(entryKey => (entryKey.Key == key || (entryKey.Key != null && entryKey.Key.Equals(key))) && typeof(T).IsAssignableFrom(entryKey.Type)))
                {
                    if (matchedEntryKey != null)
                        throw new ObjectFactoryIsAmbiguosException(typeof(T), key);

                    matchedEntryKey = matchingEntryKey;
                    factory = _factories[matchedEntryKey];
                }
            }

            if (matchedEntryKey == null)
                throw new ObjectFactoryIsNotRegisteredException(typeof(T), key);

            return () =>
            {
                if (_currentlyExecutedFactories.Contains(matchedEntryKey))
                    throw new ObjectFactoryCycleDependencyDetected(typeof(T), matchedEntryKey);

                _currentlyExecutedFactories.Add(matchedEntryKey);

                var result = (factory as Func<IIoCSession, T>)(this);

                return result;
            };
        }
    }
}