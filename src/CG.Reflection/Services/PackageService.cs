using System;
using System.Reflection;

// testing ci pipeline

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
        /// This field contains info for the calling assembly.
        /// </summary>
        private readonly Lazy<IPackageInfo> _callingAssembly;

        /// <summary>
        /// This field contains info for the entry assembly.
        /// </summary>
        private readonly Lazy<IPackageInfo> _entryAssembly;

        #endregion

        // *******************************************************************
        // Properties.
        // *******************************************************************

        #region Properties

        /// <inheritdoc />
        public IPackageInfo CallingAssembly => _callingAssembly.Value;

        /// <inheritdoc />
        public IPackageInfo EntryAssembly => _entryAssembly.Value;

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

            _callingAssembly = new Lazy<IPackageInfo>(() => 
            {
                var asm = Assembly.GetCallingAssembly();
                var obj = new PackageInfo(asm);
                return obj;
            });

            _entryAssembly = new Lazy<IPackageInfo>(() =>
            {
                var asm = Assembly.GetEntryAssembly();
                var obj = new PackageInfo(asm);
                return obj;
            });
        }

        #endregion
    }
}
