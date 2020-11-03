using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CG.Reflection.Services
{
    /// <summary>
    /// This class is a test fixture for the <see cref="PackageService"/>
    /// class.
    /// </summary>
    [TestClass]
    public class PackageServiceFixture
    {
        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <summary>
        /// This method ensures that the <see cref="PackageService.PackageService"/>
        /// constructor properly initializes object instances.
        /// </summary>
        [TestMethod]
        [TestCategory("Unit")]
        public void PackageService_Ctor()
        {
            // Arrange ...

            // Act ...
            var result = new PackageService();

            // Assert ...
            Assert.IsTrue(
                result.CallingAssembly != null,
                "The CallingExecutable property wasn't initialized by the ctor."
                );
        }

        #endregion
    }
}
