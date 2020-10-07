using System;

namespace CG.Reflection.Services
{
    /// <summary>
    /// This interface represents an object that contains good-to-know
    /// information about well-known assemblies and their components.
    /// </summary>
    public interface IPackageService
    {
        /// <summary>
        /// This property contains package information for the calling assembly.
        /// </summary>
        IPackageInfo CallingAssembly { get; }

        /// <summary>
        /// This property contains package information for the entry assembly.
        /// </summary>
        IPackageInfo EntryAssembly { get; }
    }
}
