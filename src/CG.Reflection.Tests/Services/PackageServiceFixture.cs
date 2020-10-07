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
                result.CallingExecutable != null,
                "The CallingExecutable property wasn't initialized by the ctor."
                );
            Assert.IsTrue(
                result.CallingExecutable.AssemblyVersion == null,
                "The AssemblyVersion property for the calling executable wasn't initialized by the ctor."
                );
            Assert.IsTrue(
                result.CallingExecutable.FileVersion != null,
                "The FileVersion property for the calling executable wasn't initialized by the ctor."
                );
            Assert.IsTrue(
                result.CallingExecutable.InformationalVersion != null,
                "The InformationalVersion property for the calling executable wasn't initialized by the ctor."
                );
            Assert.IsTrue(
                result.CallingExecutable.Guid == null,
                "The Guid property for the calling executable wasn't initialized by the ctor."
                );
            Assert.IsFalse(
                string.IsNullOrEmpty(result.CallingExecutable.Copyright),
                "The Copyright property for the calling executable wasn't initialized by the ctor."
                );
            Assert.IsFalse(
                string.IsNullOrEmpty(result.CallingExecutable.Company),
                "The Company property for the calling executable wasn't initialized by the ctor."
                );
            Assert.IsFalse(
                string.IsNullOrEmpty(result.CallingExecutable.Product),
                "The Product property for the calling executable wasn't initialized by the ctor."
                );
            Assert.IsFalse(
                string.IsNullOrEmpty(result.CallingExecutable.Title),
                "The Title property for the calling executable wasn't initialized by the ctor."
                );
            Assert.IsFalse(
                string.IsNullOrEmpty(result.CallingExecutable.Description),
                "The Description property for the calling executable wasn't initialized by the ctor."
                );
            Assert.IsTrue(
                string.IsNullOrEmpty(result.CallingExecutable.Trademark),
                "The Trademark property for the calling executable wasn't initialized by the ctor."
                );
        }

        #endregion
    }
}
