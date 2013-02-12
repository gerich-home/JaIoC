using System;

namespace JaIoC
{
    public class IoCEntryKey
    {
        private readonly Type _type;
        private readonly object _key;

        public IoCEntryKey(Type type, object key)
        {
            _type = type;
            _key = key;
        }

        public Type Type { get { return _type; } }
        public object Key { get { return _key; } }

        public static bool Equals(IoCEntryKey key1, IoCEntryKey key2)
        {
            if (Object.Equals(null, key1) ||
                Object.Equals(null, key2) ||
                key1.GetType() != key2.GetType())
                return false;

            if (ReferenceEquals(key1, key2))
                return true;

            return key1._type == key2._type && (key1._key == null && key2._key == null) || (key1._key != null && key1._key.Equals(key2._key));
        }

        protected bool Equals(IoCEntryKey other)
        {
            return Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return Equals(this, obj as IoCEntryKey);
        }

        public override int GetHashCode()
        {
            int hash = _type.GetHashCode();

            if (_key != null)
                hash ^= _key.GetHashCode();

            return hash;
        }

        public override string ToString()
        {
            return string.Format("IoCEntryKey {{Type: {0}{1}}}", _type, _key == null ? "" : string.Format(", Key: {0}", _key));
        }
    }
}