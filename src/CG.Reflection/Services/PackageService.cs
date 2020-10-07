using System;
using System.Reflection;

namespace CG.Reflection.Services
{
    /// <summary>
    /// This class is a default implementation of the <see cref="IPackageService"/>
    /// interface.
    /// </summary>
    public class PackageService : IPackageService
    {
        // *******************************************************************
        // Fields.
        // *******************************************************************

        #region Fields

        /// <summary>
        /// This field contains info for the calling executable.
        /// </summary>
        private readonly Lazy<IPackageInfo> _callingExecutable;

        #endregion

        // *******************************************************************
        // Properties.
        // *******************************************************************

        #region Properties

        /// <inheritdoc />
        public IPackageInfo CallingExecutable => _callingExecutable.Value;

        #endregion

        // *******************************************************************
        // Constructors.
        // *******************************************************************

        #region Constructors

        /// <summary>
        /// This constructor creates a new instance of the <see cref="PackageService"/>
        /// class.
        /// </summary>
        public PackageService()
        {
            // Lazy load the fields (we won't need them until/unless we need them).

            _callingExecutable = new Lazy<IPackageInfo>(() => 
            {
                var asm = Assembly.GetExecutingAssembly();
                var obj = new PackageInfo(asm);
                return obj;
            });
        }

        #endregion
    }
}
