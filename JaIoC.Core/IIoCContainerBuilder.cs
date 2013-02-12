namespace JaIoC
{
    public interface IIoCContainerBuilder
        : IIoCContainerBuilderForKey
        , IFluentInterface
    {
        IIoCContainerBuilderForKey ForKey(object key);

        void Finish();
    }
}