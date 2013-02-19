using JaIoC;

namespace SampleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var ioc = new IoCContainerBuilder();

            ioc.ForKey("foo").Register(c => new Foo(c.Resolve<Foo>("foo")));

            ioc.Finish();

            var x = ioc.Result.Resolve<Foo>("foo");
        }
    }

    internal class Foo
    {
        private readonly Foo _foo;

        public Foo(Foo foo)
        {
            _foo = foo;
        }
    }
}
