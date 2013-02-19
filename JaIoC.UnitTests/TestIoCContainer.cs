using System;
using JaIoC;
using JaIoc.UnitTests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JaIoc.UnitTests
{
    [TestClass]
    public class TestIoCContainer
    {
        interface IA
        {
        }

        class A : IA
        {
        }

        class C : IA
        {
        }

        interface IB
        {
        }

        class B : IB
        {
        }

        [TestMethod]
        public void TestResolvesA()
        {
            const int n = 3;

            var iocBuilder = new IoCContainerBuilder();

            int calls = 0;
            A lastA = null;

            iocBuilder.Register<IA>((c) =>
            {
                calls++;
                lastA = new A();
                return lastA;
            }); 
            iocBuilder.Finish();

            var ioc = iocBuilder.Result;

            for (int i = 1; i <= n; i++)
            {
                var a = ioc.Resolve<IA>();

                Assert.AreEqual<int>(i, calls);
                Assert.AreSame(a, lastA);
            }
        }

        [TestMethod]
        public void TestRegisterATwice()
        {
            var iocBuilder = new IoCContainerBuilder();
            int calls = 0;

            iocBuilder.Register<IA>((c) =>
            {
                calls++;
                return new A();
            });

            AssertUtils.ShouldThrow<ObjectFactoryIsAlreadyRegisteredException>(() => iocBuilder.Register<IA>((c) =>
            {
                calls++;
                return new A();
            }));

            Assert.AreEqual(0, calls);
        }

        [TestMethod]
        public void TestDoesNotResolveA()
        {
            var ioc = new IoCContainerBuilder();

            ioc.Finish();

            AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(() => ioc.Result.Resolve<IA>());
        }

        [TestMethod]
        public void TestResolvesIfRegisteredConcreteTypeA()
        {
            const int n = 3;
            int calls = 0;
            A lastA = null;

            var ioc = new IoCContainerBuilder();

            ioc.Register<A>((c) =>
            {
                calls++;
                lastA = new A();
                return lastA;
            });

            ioc.Finish();
            
            Assert.AreEqual<int>(0, calls);

            for (int i = 1; i <= n; i++)
            {
                var a = ioc.Result.Resolve<IA>();

                Assert.AreEqual<int>(i, calls);
                Assert.AreSame(a, lastA);
            }
        }

        [TestMethod]
        public void TestDoesNotResolvesAmbiguosRegistrations()
        {
            int callsA = 0;
            int callsC = 0;

            var ioc = new IoCContainerBuilder();

            ioc.Register<A>((c) =>
            {
                callsA++;
                return new A();
            });

            ioc.Register<C>((c) =>
            {
                callsC++;
                return new C();
            });

            ioc.Finish();


            AssertUtils.ShouldThrow<ObjectFactoryIsAmbiguosException>(() => ioc.Result.Resolve<IA>());

            Assert.AreEqual<int>(0, callsA);
            Assert.AreEqual<int>(0, callsC);
        }


        [TestMethod]
        public void TestDoesNotResolveAWhenBIsRegistered()
        {
            var ioc = new IoCContainerBuilder();

            bool isCalled = false;
            ioc.Register<IB>((c) => { isCalled = true; return new B(); });

            ioc.Finish();
            
            AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(
                () => ioc.Result.Resolve<IA>());

            Assert.IsFalse(isCalled);
        }

        [TestMethod]
        public void TestResolvesAB()
        {
            const int n = 3;
            int callsA = 0;
            A lastA = null;
            int callsB = 0;
            B lastB = null;

            var ioc = new IoCContainerBuilder();

            ioc.Register<IA>((c) =>
            {
                callsA++;
                lastA = new A();
                return lastA;
            });

            ioc.Register<IB>((c) =>
            {
                callsB++;
                lastB = new B();
                return lastB;
            });

            ioc.Finish();
            
            Assert.AreEqual<int>(0, callsA);
            Assert.AreEqual<int>(0, callsB);

            for (int i = 1; i <= n; i++)
            {
                var a = ioc.Result.Resolve<IA>();

                Assert.AreEqual<int>(i, callsA);
                Assert.AreEqual<int>(i - 1, callsB);
                Assert.AreSame(a, lastA);

                var b = ioc.Result.Resolve<IB>();

                Assert.AreEqual<int>(i, callsA);
                Assert.AreEqual<int>(i, callsB);
                Assert.AreSame(b, lastB);
            }
        }

        [TestMethod]
        public void TestResolvesAByStringKey()
        {
            Assert.Inconclusive();

            //const int n = 3;
            //int calls = 0;
            //A lastA = null;

            //var ioc = new IoCContainerBuilder();

            //ioc.Register<IA>("key", (c) =>
            //{
            //    calls++;
            //    lastA = new A();
            //    return lastA;
            //});

            //Assert.AreEqual<int>(0, calls);

            //for (int i = 1; i <= n; i++)
            //{
            //    var a = ioc.Result.Resolve<IA>("key");

            //    Assert.AreEqual<int>(i, calls);
            //    Assert.AreSame(a, lastA);
            //}
        }

        [TestMethod]
        public void TestDoesNotResolveAByStringKey()
        {
            var ioc = new IoCContainerBuilder();
            ioc.Finish();
            
            AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(
                () => ioc.Result.Resolve<IA>("key"));
        }

        private class Key
        {
            private readonly int _index;

            public Key(int index)
            {
                _index = index;
            }

            public static bool Equals(Key key1, Key key2)
            {
                if (Object.ReferenceEquals(key1, key2))
                    return true;

                if (Object.ReferenceEquals(null, key1) ||
                    Object.ReferenceEquals(null, key2) ||
                    key1.GetType() != key2.GetType())
                    return false;

                return key1._index == key2._index;
            }

            protected bool Equals(Key other)
            {
                return Equals(this, other);
            }

            public override bool Equals(object obj)
            {
                return Equals(this, obj as Key);
            }

            public override int GetHashCode()
            {
                return _index;
            }

            public static bool operator ==(Key key1, Key key2)
            {
                if (Object.ReferenceEquals(key1, key2))
                    return true;

                if (Object.ReferenceEquals(key1, null) || Object.ReferenceEquals(key2, null))
                    return false;

                return key1._index == key2._index;
            }

            public static bool operator !=(Key key1, Key key2)
            {
                return !(key1 == key2);
            }
        }

        [TestMethod]
        public void TestResolvesAByObjectKey()
        {
            Assert.Inconclusive();

            //const int n = 3;
            //int calls = 0;
            //A lastA = null;

            //var ioc = new IoCContainerBuilder();

            //ioc.Register<IA>(new Key(10), (c) =>
            //{
            //    calls++;
            //    lastA = new A();
            //    return lastA;
            //});

            //Assert.AreEqual<int>(0, calls);

            //for (int i = 1; i <= n; i++)
            //{
            //    var a = ioc.Result.Resolve<IA>(new Key(10));

            //    Assert.AreEqual<int>(i, calls);
            //    Assert.AreSame(a, lastA);
            //}
        }

        [TestMethod]
        public void TestDoesNotResolveAByObjectKey()
        {
            var ioc = new IoCContainerBuilder();
            ioc.Finish();

            AssertUtils.ShouldThrow<Exception>(
                () => ioc.Result.Resolve<IA>(new Key(10)));
        }

        [TestMethod]
        public void TestResolvesInstance()
        {
            Assert.Inconclusive();

            //const int n = 3;
            //int calls = 0;
            //A instance = null;

            //var ioc = new IoCContainerBuilder();

            //ioc.RegisterInstance<IA>(new Key(10), (c) =>
            //{
            //    calls++;
            //    instance = new A();
            //    return instance;
            //});

            //Assert.AreEqual<int>(0, calls);

            //for (int i = 1; i <= n; i++)
            //{
            //    var a = ioc.Result.Resolve<IA>(new Key(10));

            //    Assert.AreEqual<int>(1, calls);
            //    Assert.AreSame(a, instance);
            //}
        }
    }
}
