using System;
using System.Collections.Generic;

namespace JaIoC
{
    public class IoCContainerBuilder
        : IIoCContainerBuilderWithResult<IoCContainer>
    {
        private enum State
        {
            SettingKeyOrAddingRegistration,
            AddingRegistration,
            Finished
        }

        private State _state = State.SettingKeyOrAddingRegistration;
        private readonly IDictionary<IoCEntryKey, Func<IIoCSession, object>> _registrations = new Dictionary<IoCEntryKey, Func<IIoCSession, object>>();
        private IoCContainer _result;
        private object _key;

        public IIoCContainerBuilderForKey ForKey(object key)
        {
            if (_state != State.SettingKeyOrAddingRegistration)
                throw new InvalidOperationException("Invalid IoCContainerBuilder state");

            _key = key;

            _state = State.AddingRegistration;

            return this;
        }

        object IIoCContainerBuilderForKey.Key
        {
            get { return _key; }
        }

        public IIoCContainerBuilder Register<T>(Func<IIoCSession, T> factory)
            where T : class
        {
            if (_state == State.Finished)
                throw new InvalidOperationException("Invalid IoCContainerBuilder state");

            var iocKey = new IoCEntryKey(typeof(T), _key);

            if (_registrations.ContainsKey(iocKey))
                throw new ObjectFactoryIsAlreadyRegisteredException();
            
            _registrations.Add(iocKey, factory);

            _key = null;

            _state = State.SettingKeyOrAddingRegistration;

            return this;
        }

        public void Finish()
        {
            if (_state != State.SettingKeyOrAddingRegistration)
                throw new InvalidOperationException("Invalid IoCContainerBuilder state");

            Result = new IoCContainer(_registrations);

            _state = State.Finished;
        }

        public IoCContainer Result
        {
            get
            {
                if (_state != State.Finished)
                    throw new InvalidOperationException("Invalid IoCContainerBuilder state");
                
                return _result;
            }
            private set { _result = value; }
        }
    }
}