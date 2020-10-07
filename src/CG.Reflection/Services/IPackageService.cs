using System;

namespace CG.Reflection.Services
{
    /// <summary>
    /// This interface represents an object that contains good-to-know
    /// information about the current application and its components.
    /// </summary>
    public interface IPackageService
    {
        /// <summary>
        /// This property contains package information for the calling executable.
        /// </summary>
        IPackageInfo CallingExecutable { get; }
    }
}
