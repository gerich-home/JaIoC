using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using JaIoC;

namespace JaIoc.UnitTests
{
    [TestClass]
    public class TestIoCContainerExtensions
    {
        interface IA
        {
        }

        class A : IA
        {
        }

        interface IB
        {
        }

        class B : IB
        {
            public static Action<B> ConstructorAction;

            public B()
            {
                ConstructorAction(this);
            }
        }

        [TestMethod]
        public void TestSingletoneExtension()
        {
            var iocBuilderMock = new Mock<IIoCContainerBuilderForKey>();

            int registerCalls = 0;
            Func<IIoCSession, IA> registeredFactory = null;

            iocBuilderMock
                .Setup(t => t.Register(It.IsAny<Func<IIoCSession, IA>>()))
                .Callback<Func<IIoCSession, IA>>((factory) =>
                                                     {
                                                         registerCalls++;
                                                         registeredFactory = factory;
                                                     });

            var iocBuilder = iocBuilderMock.Object;

            


            var iocSessionMock = new Mock<IIoCSession>();

            iocSessionMock
                .Setup(t => t.FactoryFor<IA>(It.IsAny<object>()))
                .Verifiable();

            var iocSession = iocSessionMock.Object;

            
            
            const int n = 3;
            int calls = 0;
            A lastA = null;

            iocBuilder.RegisterInstance<IA>((c) =>
                                                {
                                                    calls++;
                                                    lastA = new A();
                                                    return lastA;
                                                });

            
            
            
            Assert.AreEqual<int>(0, calls);
            Assert.AreEqual<int>(1, registerCalls);
            Assert.IsNotNull(registeredFactory);

            for (int i = 1; i <= n; i++)
            {
                var a = registeredFactory(iocSession);

                Assert.AreEqual<int>(1, calls);

                iocSessionMock
                    .Verify(t => t.FactoryFor<IA>(It.IsAny<object>()), Times.Never());

                Assert.AreSame(a, lastA);
            }

            Assert.AreEqual<int>(1, registerCalls);
        }

        private void TestResolvesExtensionForSessionImpl(object expectedKey)
        {
            A lastA = null;
            int factoryCalls = 0;
            Func<IA> factory = () =>
                                   {
                                       factoryCalls++;
                                       lastA = new A();
                                       return lastA;
                                   };



            int factoryForCalls = 0;
            object factoryForKey = null;

            var iocSessionMock = new Mock<IIoCSession>();
            
            iocSessionMock
                .Setup(t => t.FactoryFor<IA>(It.IsAny<object>()))
                .Returns<object>(key =>
                                     {
                                         factoryForCalls++;
                                         factoryForKey = key;
                                         return factory;
                                     });

            var iocSession = iocSessionMock.Object;



            var a = iocSession.Resolve<IA>(expectedKey);

            Assert.AreEqual(1, factoryForCalls);
            Assert.AreEqual(1, factoryCalls);
            Assert.AreEqual(expectedKey, factoryForKey);
            Assert.AreSame(lastA, a);
        }

        [TestMethod]
        public void TestResolvesExtensionForSession()
        {
            TestResolvesExtensionForSessionImpl(null);

            TestResolvesExtensionForSessionImpl("test");

            TestResolvesExtensionForSessionImpl(new object());
        }

        private void TestResolvesExtensionForIoCContainerImpl(object expectedKey)
        {
            A lastA = null;
            int factoryCalls = 0;
            Func<IA> factory = () =>
            {
                factoryCalls++;
                lastA = new A();
                return lastA;
            };



            int factoryForCalls = 0;
            object factoryForKey = null;

            var iocSessionMock = new Mock<IIoCSession>();

            iocSessionMock
                .Setup(t => t.FactoryFor<IA>(It.IsAny<object>()))
                .Returns<object>(key =>
                {
                    factoryForCalls++;
                    factoryForKey = key;
                    return factory;
                });

            var iocSession = iocSessionMock.Object;



            int startCalls = 0;

            var iocContainerMock = new Mock<IIoCContainer>();

            iocContainerMock
                .Setup(t => t.Start())
                .Returns(() =>
                {
                    startCalls++;
                    return iocSession;
                });

            var iocContainer = iocContainerMock.Object;


            var a = iocContainer.Resolve<IA>(expectedKey);

            Assert.AreEqual(1, startCalls);
            Assert.AreEqual(1, factoryForCalls);
            Assert.AreEqual(1, factoryCalls);
            Assert.AreEqual(expectedKey, factoryForKey);
            Assert.AreSame(lastA, a);
        }

        [TestMethod]
        public void TestResolvesExtensionForIoCContainer()
        {
            TestResolvesExtensionForIoCContainerImpl(null);

            TestResolvesExtensionForIoCContainerImpl("test");

            TestResolvesExtensionForIoCContainerImpl(new object());
        }

        [TestMethod]
        public void TestRegisterTypeExtension()
        {
            try
            {

                var iocBuilderMock = new Mock<IIoCContainerBuilderForKey>();

                int registerCalls = 0;
                Func<IIoCSession, B> registeredFactory = null;

                iocBuilderMock
                    .Setup(t => t.Register(It.IsAny<Func<IIoCSession, B>>()))
                    .Callback<Func<IIoCSession, B>>((factory) =>
                                                        {
                                                            registerCalls++;
                                                            registeredFactory = factory;
                                                        });

                var iocBuilder = iocBuilderMock.Object;


                var iocSessionMock = new Mock<IIoCSession>();

                iocSessionMock
                    .Setup(t => t.FactoryFor<IB>(It.IsAny<object>()))
                    .Verifiable();

                const int n = 3;

                int calls = 0;
                B lastB = null;

                B.ConstructorAction = (b) =>
                                          {
                                              calls++;
                                              lastB = b;
                                          };


                iocBuilder.RegisterType<B>();

                Assert.AreEqual(0, calls);

                Assert.AreEqual<int>(1, registerCalls);
                Assert.IsNotNull(registeredFactory);

                var iocSession = iocSessionMock.Object;

                for (int i = 1; i <= n; i++)
                {
                    var b = registeredFactory(iocSession);

                    Assert.AreEqual(i, calls);

                    iocSessionMock
                        .Verify(t => t.FactoryFor<IB>(It.IsAny<object>()), Times.Never());

                    Assert.AreSame(b, lastB);
                }

                Assert.AreEqual<int>(1, registerCalls);
            }
            finally
            {
                B.ConstructorAction = null;
            }
        }
    }
}