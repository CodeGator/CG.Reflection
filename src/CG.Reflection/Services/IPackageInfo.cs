using System;

namespace CG.Reflection.Services
{
    /// <summary>
    /// This interface represents an object that contains package 
    /// information about a .NET assembly.
    /// </summary>
    public interface IPackageInfo
    {
        /// <summary>
        /// This property contains the assembly version for the package.
        /// </summary>
        Version AssemblyVersion { get; }

        /// <summary>
        /// This property contains the file version for the package.
        /// </summary>
        Version FileVersion { get; }

        /// <summary>
        /// This property contains the nuget version for the package.
        /// </summary>
        Version InformationalVersion { get; }

        /// <summary>
        /// This property contains the copyright for the package.
        /// </summary>
        string Copyright { get; }

        /// <summary>
        /// This property contains the product name for the package.
        /// </summary>
        string Product { get; }

        /// <summary>
        /// This property contains the description for the package.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// This property contains the company name for the package.
        /// </summary>
        string Company { get; }

        /// <summary>
        /// This property contains the GUID for the package.
        /// </summary>
        Guid? Guid { get; }

        /// <summary>
        /// This property contains the title for the package.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// This property contains the trademark for the package.
        /// </summary>
        string Trademark { get; }
    }
}
