using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JaIoc.UnitTests.Utils
{
    public static class AssertUtils
    {
        public static void ShouldThrow<T>(Action action, string message = null)
            where T : Exception
        {
            try
            {
                action();
            }
            catch (T)
            {
                return;
            }

            if(message == null)
                Assert.Fail("Should throw exception of type {0}", typeof(T));
            else
                Assert.Fail(message);
        }
    }
}
