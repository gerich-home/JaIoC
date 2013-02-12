using System;
using System.Linq.Expressions;

namespace JaIoC
{
    public static class ReflectionUtility
    {
        public static string PropertyName<T, TReturn>(Expression<Func<T, TReturn>> expression)
        {
            var body = (MemberExpression)expression.Body;
            return body.Member.Name;
        }

        public static string MethodName<T, TReturn>(Expression<Func<T, TReturn>> expression)
        {
            var body = (MethodCallExpression)expression.Body;
            return body.Method.Name;
        }

        public static string MethodName<T>(Expression<Action<T>> expression)
        {
            var body = (MethodCallExpression)expression.Body;
            return body.Method.Name;
        }

        public static string MethodName(Expression<Action> expression)
        {
            var body = (MethodCallExpression)expression.Body;
            return body.Method.Name;
        }
    }
}