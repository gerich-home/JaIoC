using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JaIoC;

namespace JaIoC
{
    public static class IIoCContainerExtensions
    {
        public static T Resolve<T>(this IIoCContainer c, object key = null)
            where T : class
        {
            var session = c.Start();
            return session.Resolve<T>(key);
        }

        public static T Resolve<T>(this IIoCSession c, object key = null)
            where T : class
        {
            var factory = c.FactoryFor<T>(key);
            return factory();
        }

        public static void RegisterInstance<T>(this IIoCContainerBuilderForKey c, Func<IIoCSession, T> factory)
            where T : class
        {
            T instance = null;
            c.Register(x => instance ?? (instance = factory(x)));
        }

        private static Func<object> GetFactoryForParameter(IIoCSession ioc, Type parameterType)
        {
            var parameterTypeArray = new[] { parameterType };

            var factoryForMethodName = ReflectionUtility.MethodName((IIoCSession t) => t.FactoryFor<object>(null));
            var iocFactoryForMethodInfo = typeof(IIoCSession).GetMethod(factoryForMethodName);
            var iocFactoryForParameterTypeMethodInfo = iocFactoryForMethodInfo.MakeGenericMethod(parameterTypeArray);

            var returnType = typeof(Func<>).MakeGenericType(parameterTypeArray);
            var delegateType = typeof(Func<,>).MakeGenericType(new[] { typeof(object), returnType });
            var iocFactoryForParameterType = (Func<object, Func<object>>)Delegate.CreateDelegate(delegateType, ioc, iocFactoryForParameterTypeMethodInfo);

            return iocFactoryForParameterType(null);
        }

        public static void RegisterType<T, TImplementation>(this IIoCContainerBuilderForKey c)
            where T : class
            where TImplementation : T
        {
            if (typeof(TImplementation).IsAbstract)
            {
                throw new ArgumentException("TImplementation should not be an abstract type");
            }

            if (typeof(TImplementation).GetConstructors().Length == 0)
            {
                throw new ArgumentException("TImplementation should define at least one public constructor");
            }

            c.Register(x =>
            {
                var constructors = typeof(TImplementation).GetConstructors();
                var caughtExceptions = new Dictionary<ConstructorInfo, IoCException>();

                foreach (var constructor in constructors.OrderByDescending(info => info.GetParameters().Count()))
                {
                    var parameters = constructor.GetParameters();

                    var factories = new Func<object>[parameters.Length];

                    bool skipConstructor = false;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        try
                        {
                            var factory = GetFactoryForParameter(x, parameters[i].ParameterType);
                            factories[i] = factory;

                        }
                        catch (IoCException e)
                        {
                            if (constructors.Length == 1)
                            {
                                throw new ParameterNotResolvedException(constructor, i, e, typeof(TImplementation), c.Key);
                            }

                            caughtExceptions.Add(constructor, e);
                            skipConstructor = true;
                        }
                    }

                    if (!skipConstructor)
                    {
                        return constructor.Invoke(factories.Select(f => f()).ToArray()) as T;
                    }
                }

                throw new NoAppropriateContructorException(typeof(TImplementation), c.Key, caughtExceptions);
            });
        }

        public static void RegisterType<T>(this IIoCContainerBuilderForKey c)
            where T : class
        {
            RegisterType<T, T>(c);
        }

        public static void RegisterConstructor<T, TImplementation>(this IIoCContainerBuilderForKey c, Expression<Func<IIoCSession, TImplementation>> constructorExpression)
            where T : class
            where TImplementation : T
        {
            if (constructorExpression.NodeType != ExpressionType.Lambda)
                throw new ArgumentException("constructorExpression");

            if (constructorExpression.Body.NodeType != ExpressionType.New)
                throw new ArgumentException("constructorExpression");

            var newExpression = constructorExpression.Body as NewExpression;
            if (newExpression == null)
                throw new ArgumentException("constructorExpression");

            c.Register(x =>
            {
                var constructor = newExpression.Constructor;
                var parameters = constructor.GetParameters();

                var factories = new Func<object>[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    var constructorArgument = newExpression.Arguments[i];

                    try
                    {
                        switch (constructorArgument.NodeType)
                        {
                            case ExpressionType.Constant:
                                {
                                    var constantExpression = constructorArgument as ConstantExpression;
                                    if (constantExpression != null)
                                    {
                                        if (constantExpression.Value == null)
                                        {
                                            var factory = GetFactoryForParameter(x, parameters[i].ParameterType);
                                            factories[i] = factory;
                                            continue;
                                        }
                                    }
                                }
                                break;
                            case ExpressionType.Call:
                                {
                                    Expression<Func<IIoCSession, object>> lambda;

                                    if (SetterValueExpression(constructorArgument, parameters[i].ParameterType, out lambda))
                                    {
                                        var factory = lambda.Compile();
                                        factories[i] = () => factory(x);
                                        continue;
                                    }
                                }
                                break;
                            case ExpressionType.Convert:
                                {
                                    var convertExpression = constructorArgument as UnaryExpression;
                                    if (convertExpression != null)
                                    {
                                        Expression<Func<IIoCSession, object>> lambda;

                                        if (SetterValueExpression(convertExpression.Operand, parameters[i].ParameterType, out lambda))
                                        {
                                            var factory = lambda.Compile();
                                            factories[i] = () => factory(x);
                                            continue;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    catch (IoCException e)
                    {
                        throw new ParameterNotResolvedException(constructor, i, e, typeof(TImplementation), c.Key);
                    }

                    throw new ParameterNotResolvedException(constructor, i, null, typeof(TImplementation), c.Key);
                }

                return constructor.Invoke(factories.Select(f => f()).ToArray()) as T;
            });
        }

        public static void RegisterConstructor<T>(this IIoCContainerBuilderForKey c, Expression<Func<IIoCSession, T>> constructorExpression)
            where T : class
        {
            RegisterConstructor<T, T>(c, constructorExpression);
        }

        private static bool SetterValueExpression(Expression expression, Type targetType, out Expression<Func<IIoCSession, object>> lambda)
        {
            bool success = false;
            lambda = null;

            var callExpression = expression as MethodCallExpression;
            if (callExpression != null)
            {
                var setEqualMethod = typeof(Set).GetMethod(ReflectionUtility.MethodName(() => Set.Equal(0)));
                if (callExpression.Method.GetGenericMethodDefinition() == setEqualMethod.GetGenericMethodDefinition())
                {
                    var valueExpression = callExpression.Arguments[0];
                    var v = new GetParameterVariableName();
                    v.Visit(valueExpression);
                    lambda = Expression.Lambda<Func<IIoCSession, object>>(
                        Expression.Convert(
                            Expression.Convert(
                                callExpression.Arguments[0],
                                targetType),
                            typeof(object)
                        ).Reduce(),
                        new[] { v.IoCParameter ?? Expression.Parameter(typeof(IIoCSession)) });

                    success = true;
                }
            }

            return success;
        }

        private class GetParameterVariableName : ExpressionVisitor
        {
            public ParameterExpression IoCParameter { get; private set; }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node.Type == typeof(IIoCSession))
                {
                    if (IoCParameter == null)
                    {
                        IoCParameter = node;
                    }
                    else if (IoCParameter != node)
                    {
                        throw new InvalidOperationException();
                    }
                }

                return base.VisitParameter(node);
            }
        }
    }
}