using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CG.Reflection.Extensions
{
    public class A { }
    public class B : A { }

    /// <summary>
    /// This class is used for internal testing purposes.
    /// </summary>
    public static class _TestHelper
    {
        /// <summary>
        /// This method is used for internal testing purposes.
        /// </summary>
        /// <param name="value">This parameter is used for internal testing purposes.</param>
        public static void TestMethod1(this int value) { }

        /// <summary>
        /// This method is used for internal testing purposes.
        /// </summary>
        /// <param name="value">This parameter is used for internal testing purposes.</param>
        /// <param name="a">This parameter is used for internal testing purposes.</param>
        public static void TestMethod2(this int value, string a = "test") { }

        /// <summary>
        /// This method is used for internal testing purposes.
        /// </summary>
        /// <param name="value">This parameter is used for internal testing purposes.</param>
        /// <param name="a">This parameter is used for internal testing purposes.</param>
        public static void TestMethod3<T>(this int value, string a = "test") where T : A { }
    }

    /// <summary>
    /// This class is a test fixture for the <see cref="AppDomainExtensions"/>
    /// class.
    /// </summary>
    [TestClass]
    [TestCategory("Unit")]
    public class AppDomainExtensionsFixture
    {
        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <summary>
        /// This method verifies that the <see cref="AppDomainExtensions.ExtensionMethods(AppDomain, Type, string, Type[], string, string)"/>
        /// method can locate a test extension method with no parameters.
        /// </summary>
        [TestMethod]
        public void AppDomainExtensions_ExtensionMethods1()
        {
            // Arrange ...

            // Act ...
            var result = AppDomain.CurrentDomain.ExtensionMethods(
                typeof(int),
                "TestMethod1"
                );

            // Assert ...
            Assert.IsTrue(result != null, "method returned an invalid value.");
        }

        // *******************************************************************

        /// <summary>
        /// This method verifies that the <see cref="AppDomainExtensions.ExtensionMethods(AppDomain, Type, string, Type[], string, string)"/>
        /// method can locate a test extension method with parameters.
        /// </summary>
        [TestMethod]
        public void AppDomainExtensions_ExtensionMethods2()
        {
            // Arrange ...

            // Act ...
            var result = AppDomain.CurrentDomain.ExtensionMethods(
                typeof(int),
                "TestMethod2",
                new Type[] { typeof(string) }
                );

            // Assert ...
            Assert.IsTrue(result != null, "method returned an invalid value.");
        }

        // *******************************************************************

        /// <summary>
        /// This method verifies that the <see cref="AppDomainExtensions.ExtensionMethods{T}(AppDomain, Type, string, Type[], string, string)"/>
        /// method can locate a test extension method with parameters and generic
        /// type arguments.
        /// </summary>
        [TestMethod]
        public void AppDomainExtensions_ExtensionMethods3()
        {
            // Arrange ...

            // Act ...
            var result = AppDomain.CurrentDomain.ExtensionMethods<B>(
                typeof(int),
                "TestMethod3",
                new Type[] { typeof(string) }
                );

            // Assert ...
            Assert.IsTrue(result != null, "method returned an invalid value.");
        }

        #endregion
    }
}
