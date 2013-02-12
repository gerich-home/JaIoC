using System;
using System.ComponentModel;

namespace JaIoC
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IFluentInterface
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        string ToString();
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object obj);
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        Type GetType();
    }
}
