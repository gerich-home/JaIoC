using JaIoC;

namespace JaIoC
{
    public interface IIoCContainerBuilderWithResult<out TResult>
        : IIoCContainerBuilder
        where TResult : IIoCContainer
    {
        TResult Result { get; }
    }
}