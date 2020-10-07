using System;
using System.Linq;
using System.Reflection;

namespace CG.Reflection.Services
{
    /// <summary>
    /// This class is a default implementation of the <see cref="IPackageInfo"/>
    /// interface.
    /// </summary>
    internal class PackageInfo : IPackageInfo
    {
        // *******************************************************************
        // Properties.
        // *******************************************************************

        #region Properties

        /// <inheritdoc />
        public Version AssemblyVersion { get; }

        /// <inheritdoc />
        public Version FileVersion { get; }

        /// <inheritdoc />
        public Version InformationalVersion { get; }

        /// <inheritdoc />
        public string Copyright { get; }

        /// <inheritdoc />
        public string Product { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public string Company { get; }

        /// <inheritdoc />
        public Guid? Guid { get; }

        /// <inheritdoc />
        public string Title { get; }

        /// <inheritdoc />
        public string Trademark { get; }

        #endregion

        // *******************************************************************
        // Constructors.
        // *******************************************************************

        #region Constructors

        /// <summary>
        /// This constructor creates a new instance of the <see cref="PackageInfo"/>
        /// class.
        /// </summary>
        /// <param name="assembly">the assembly to use for the operation.</param>
        public PackageInfo(
            Assembly assembly
            )
        {
            // Read the custom attributes for the package.

            if (Version.TryParse(assembly.ReadAssemblyVersion(), out var version))
            {
                AssemblyVersion = version;
            }

            if (Version.TryParse(assembly.ReadFileVersion(), out version))
            {
                FileVersion = version;
            }

            if (Version.TryParse(assembly.ReadInformationalVersion(), out version))
            {
                InformationalVersion = version;
            }

            Copyright = assembly.ReadCopyright();
            Product = assembly.ReadProduct();
            Description = assembly.ReadDescription();
            Company = assembly.ReadCompany();
            
            if (System.Guid.TryParse(assembly.ReadGuid(), out var g))
            {
                Guid = g;
            }

            Title = assembly.ReadTitle();
            Trademark = assembly.ReadTrademark();
        }

        #endregion
    }
}
