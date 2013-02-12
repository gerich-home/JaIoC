using System;
using JaIoc.UnitTests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JaIoC;

namespace JaIoc.UnitTests
{
    [TestClass]
    public class TestIoCContainerBuilder
    {
        [TestMethod]
        public void TestAccessResultBeforeFinish()
        {
            var ioc = new IoCContainerBuilder();

            AssertUtils.ShouldThrow<InvalidOperationException>(() =>
            {
                IIoCContainer x = ioc.Result;
            });
        }

        [TestMethod]
        public void TestAccessResultAfterFinish()
        {
            var ioc = new IoCContainerBuilder();

            ioc.Finish();

            IIoCContainer x = ioc.Result;
            IIoCContainer y = ioc.Result;

            Assert.IsNotNull(x);
            Assert.AreSame(x, y);
        }

        [TestMethod]
        public void TestForKeyReturnsNotNull()
        {
            var ioc = new IoCContainerBuilder();

            IIoCContainerBuilderForKey b = ioc.ForKey("test");

            Assert.IsNotNull(b);
        }

        [TestMethod]
        public void TestCannotRegisterAfterFinish()
        {
            var ioc = new IoCContainerBuilder();

            ioc.Finish();

            int calls = 0;

            AssertUtils.ShouldThrow<InvalidOperationException>(() => ioc.Register(c =>
            {
                calls++;
                return new A();
            }));

            Assert.AreEqual(0, calls);
        }

        [TestMethod]
        public void TestCannotGiveKeyAfterFinish()
        {
            var ioc = new IoCContainerBuilder();

            ioc.Finish();


            AssertUtils.ShouldThrow<InvalidOperationException>(() => ioc.ForKey("hello"));
        }

        [TestMethod]
        public void TestCannotFinishAfterFinish()
        {
            var ioc = new IoCContainerBuilder();

            ioc.Finish();

            AssertUtils.ShouldThrow<InvalidOperationException>(ioc.Finish);
        }

        [TestMethod]
        public void TestCantGiveKeyAfterKeyGiven()
        {
            var ioc = new IoCContainerBuilder();

            ioc.ForKey("hello");

            AssertUtils.ShouldThrow<InvalidOperationException>(() => ioc.ForKey("x"));
        }

        [TestMethod]
        public void TestCantFinishAfterKeyGiven()
        {
            var ioc = new IoCContainerBuilder();

            ioc.ForKey("hello");

            AssertUtils.ShouldThrow<InvalidOperationException>(ioc.Finish);
        }

        [TestMethod]
        public void TestRegisterSameTypes()
        {
            var ioc = new IoCContainerBuilder();

            int calls = 0;

            ioc.Register(c =>
            {
                calls++;
                return new A();
            });

            AssertUtils.ShouldThrow<ObjectFactoryIsAlreadyRegisteredException>(() => ioc.Register(c =>
            {
                calls++;
                return new A();
            }));

            Assert.AreEqual(0, calls);
        }

        [TestMethod]
        public void TestRegisterSameTypesWithSameKeys()
        {
            var ioc = new IoCContainerBuilder();

            int calls = 0;
            ioc.ForKey("same").Register(c =>
            {
                calls++;
                return new A();
            });
            var b = ioc.ForKey("same");


            AssertUtils.ShouldThrow<ObjectFactoryIsAlreadyRegisteredException>(() => b.Register(c =>
            {
                calls++;
                return new A();
            }));

            Assert.AreEqual(0, calls);
        }

        [TestMethod]
        public void TestRegisterDifferentTypes()
        {
            var ioc = new IoCContainerBuilder();

            int callsA = 0;
            int callsB = 0;
            A lastA = null;
            B lastB = null;

            ioc.Register(c =>
            {
                callsA++;
                lastA = new A();
                return lastA;
            });

            ioc.Register(c =>
            {
                callsB++;
                lastB = new B();
                return lastB;
            });

            ioc.Finish();

            Assert.IsNotNull(ioc.Result);

            Assert.AreEqual(0, callsA);
            Assert.AreEqual(0, callsB);


            var a = ioc.Result.Start().FactoryFor<A>()();

            Assert.AreEqual(1, callsA);
            Assert.AreEqual(0, callsB);

            Assert.AreSame(lastA, a);

            var b = ioc.Result.Start().FactoryFor<B>()();

            Assert.AreEqual(1, callsA);
            Assert.AreEqual(1, callsB);

            Assert.AreSame(lastB, b);

            {
                var s = ioc.Result.Start();
                AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(() => s.FactoryFor<C>());
            }
            {
                var s = ioc.Result.Start();
                AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(() => s.FactoryFor<A>("test"));
            }
            {
                var s = ioc.Result.Start();
                AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(() => s.FactoryFor<B>("test"));
            }

            Assert.AreEqual(1, callsA);
            Assert.AreEqual(1, callsB);
        }

        [TestMethod]
        public void TestRegisterDifferentKeys()
        {
            var ioc = new IoCContainerBuilder();

            int callsWithoutKey = 0;
            int callsWithKey = 0;
            A lastAWithoutKey = null;
            A lastAWithKey = null;

            ioc.Register(c =>
                             {
                                 callsWithoutKey++;
                                 lastAWithoutKey = new A();
                                 return lastAWithoutKey;
                             });

            ioc.ForKey("some key").Register(c =>
                                                {
                                                    callsWithKey++;
                                                    lastAWithKey = new A();
                                                    return lastAWithKey;
                                                });

            ioc.Finish();

            Assert.IsNotNull(ioc.Result);

            Assert.AreEqual(0, callsWithoutKey);
            Assert.AreEqual(0, callsWithKey);

            var aWithoutKey = ioc.Result.Start().FactoryFor<A>()();

            Assert.AreEqual(1, callsWithoutKey);
            Assert.AreEqual(0, callsWithKey);

            Assert.AreSame(lastAWithoutKey, aWithoutKey);

            var aWithKey = ioc.Result.Start().FactoryFor<A>("some key")();

            Assert.AreEqual(1, callsWithoutKey);
            Assert.AreEqual(1, callsWithKey);

            Assert.AreSame(lastAWithKey, aWithKey);

            {
                var s = ioc.Result.Start();
                AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(() => s.FactoryFor<C>());
            }

            {
                var s = ioc.Result.Start();
                AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(() => s.FactoryFor<A>("test"));
            }

            {
                var s = ioc.Result.Start();
                AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(() => s.FactoryFor<B>("some key"));
            }

            Assert.AreEqual(1, callsWithoutKey);
            Assert.AreEqual(1, callsWithKey);
        }

        [TestMethod]
        public void TestRegisterDifferentTypesWithSameKeys()
        {
            var ioc = new IoCContainerBuilder();

            int callsA = 0;
            int callsB = 0;
            A lastA = null;
            B lastB = null;
            ioc.ForKey("same").Register(c =>
            {
                callsA++;
                lastA = new A();
                return lastA;
            });

            ioc.ForKey("same").Register(c =>
            {
                callsB++;
                lastB = new B();
                return lastB;
            });

            ioc.Finish();

            Assert.IsNotNull(ioc.Result);

            Assert.AreEqual(0, callsA);
            Assert.AreEqual(0, callsB);


            var a = ioc.Result.Start().FactoryFor<A>("same")();

            Assert.AreEqual(1, callsA);
            Assert.AreEqual(0, callsB);

            Assert.AreSame(lastA, a);

            var b = ioc.Result.Start().FactoryFor<B>("same")();

            Assert.AreEqual(1, callsA);
            Assert.AreEqual(1, callsB);

            Assert.AreSame(lastB, b);

            {
                var s = ioc.Result.Start();
                AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(() => s.FactoryFor<C>());
            }

            {
                var s = ioc.Result.Start();
                AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(() => s.FactoryFor<A>("test"));
            }

            {
                var s = ioc.Result.Start();
                AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(() => s.FactoryFor<B>("test"));
            }

            {
                var s = ioc.Result.Start();
                AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(() => s.FactoryFor<A>());
            }

            {
                var s = ioc.Result.Start();
                AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(() => s.FactoryFor<B>());
            }

            Assert.AreEqual(1, callsA);
            Assert.AreEqual(1, callsB);
        }

        [TestMethod]
        public void TestWorksForDifferentTypesAndKeys()
        {
            var ioc = new IoCContainerBuilder();

            int callsA = 0;
            int callsB = 0;
            A lastA = null;
            B lastB = null;
            ioc.ForKey("a").Register(c =>
            {
                callsA++;
                lastA = new A();
                return lastA;
            });

            ioc.ForKey("b").Register(c =>
            {
                callsB++;
                lastB = new B();
                return lastB;
            });

            ioc.Finish();

            Assert.IsNotNull(ioc.Result);

            Assert.AreEqual(0, callsA);
            Assert.AreEqual(0, callsB);


            var a = ioc.Result.Start().FactoryFor<A>("a")();

            Assert.AreEqual(1, callsA);
            Assert.AreEqual(0, callsB);

            Assert.AreSame(lastA, a);

            var b = ioc.Result.Start().FactoryFor<B>("b")();

            Assert.AreEqual(1, callsA);
            Assert.AreEqual(1, callsB);

            Assert.AreSame(lastB, b);


            {
                var s = ioc.Result.Start();
                AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(() => s.FactoryFor<A>());
            }

            {
                var s = ioc.Result.Start();
                AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(() => s.FactoryFor<B>());
            }

            {
                var s = ioc.Result.Start();
                AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(() => s.FactoryFor<A>("b"));
            }

            {
                var s = ioc.Result.Start();
                AssertUtils.ShouldThrow<ObjectFactoryIsNotRegisteredException>(() => s.FactoryFor<B>("a"));
            }

            Assert.AreEqual(1, callsA);
            Assert.AreEqual(1, callsB);
        }

        public class A
        {
        }

        public class B
        {
        }

        public class C
        {
        }
    }
}