using CG.Collections.Generic;
using CG.Validations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CG.Reflection
{
    /// <summary>
    /// This class contains extension methods related to the <see cref="AppDomain"/>
    /// type, for registering types related to reflection.
    /// </summary>
    public static partial class ReflectionAppDomainExtensions
    {
        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <summary>
        /// This method searches among the assemblies loaded into the current 
        /// app-domain for any public extension methods associated with the 
        /// specified type and signature.
        /// </summary>
        /// <param name="appDomain">The application domain to use for the operation.</param>
        /// <param name="extensionType">The type to match against.</param>
        /// <param name="methodName">The method name to match against.</param>
        /// <param name="parameterTypes">An optional list of parameter type(s) 
        /// to match against.</param>
        /// <param name="assemblyWhiteList">An optional white list of assembly
        /// names - for narrowing the range of assemblies searched.</param>
        /// <param name="assemblyBlackList">An optional white list of assembly
        /// names - for narrowing the range of assemblies searched.</param>
        /// <returns>A sequence of <see cref="MethodInfo"/> objects representing
        /// zero or more matching extension methods.</returns>
        public static IEnumerable<MethodInfo> ExtensionMethods(
            this AppDomain appDomain,
            Type extensionType,
            string methodName,
            Type[] parameterTypes = null,
            string assemblyWhiteList = "",
            string assemblyBlackList = "Microsoft*, System*, mscorlib, netstandard"
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(appDomain, nameof(appDomain))
                .ThrowIfNull(extensionType, nameof(extensionType))
                .ThrowIfNullOrEmpty(methodName, nameof(methodName));

            // Should we supply a valid value?
            if (parameterTypes == null)
            {
                parameterTypes = new Type[0];
            }

            // Get the list of currently loaded assemblies.
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

            // Was a white list specified?
            if (!string.IsNullOrEmpty(assemblyWhiteList))
            {
                // Look for assemblies in the white list we might need to load.
                var toLoad = assemblyWhiteList.Split(',').Where(
                    x => assemblies.Any(y => !y.GetName().Name.IsMatch(x))
                    ).ToList();

                // Did we find any?
                if (toLoad.Any())
                {
                    // Loop and load any missing whitelisted assemblies.
                    toLoad.ForEach(x =>
                    {
                        // Look for matching files.
                        var files = Directory.GetFiles(
                            AppDomain.CurrentDomain.BaseDirectory,
                            x.EndsWith(".dll") ? x : $"{x}.dll"
                            ).ToList();

                        // Loop through the files.
                        foreach (var file in files)
                        {
                            try
                            {
                                Assembly.LoadFile(file);
                            }
                            catch (Exception)
                            {
                                // Don't care, just won't search this file.
                            }
                        }
                    });

                    // Get the list of currently loaded assemblies.
                    assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

                    // Filter out anything that doesn't belong.
                    assemblies = assemblies.ApplyWhiteList(
                        x => x.GetName().Name, assemblyWhiteList
                        ).ToList();
                }

                // At this point the assembly list should only contain assemblies 
                //   that are already loaded in the app-domain, and have their 
                //   name(s) on the white list.
            }

            // Was a black list specified?
            if (!string.IsNullOrEmpty(assemblyBlackList))
            {
                // Split the black list into parts.
                var blackParts = assemblyBlackList.Split(',');

                // Filter out anything that doesn't belong.
                assemblies = assemblies.ApplyBlackList(
                    x => x.GetName().Name, assemblyBlackList
                    ).ToList();

                // At this point the assembly list should only contains those
                //   assemblies that are already loaded in the app-domain and/or
                //   white listed, and are not on the black list.
            }

            // Now we have a list of assemblies that contains juuuuuust the right items
            //   and we can use that list to perform our next search.

            var methods = new List<MethodInfo>();

            // Create options for the parallel operation.
            var options = new ParallelOptions()
            {
#if DEBUG
                MaxDegreeOfParallelism = 1 // <-- to make debugging easier.
#else
                MaxDegreeOfParallelism = Environment.ProcessorCount
#endif
            };

            // We should be able to conduct the search in parallel.
            Parallel.ForEach(assemblies, options, (assembly) =>
            {
                // Look for types that are public, sealed, non-nested, non-generic classes.
                var types = assembly.GetTypes().Where(x => 
                    x.IsClass && x.IsPublic && 
                    x.IsSealed && !x.IsNested && 
                    !x.IsGenericType
                    ).ToList();

                // Loop through each matching type.
                foreach (var type in types)
                {
                    // Look for any public extension methods with a matching name that
                    //   doesn't have generic type arguments.
                    var typeMethods = type.GetMethods(
                        BindingFlags.Static | 
                        BindingFlags.Public
                        ).Where(x => 
                            x.Name == methodName && 
                            x.IsDefined(typeof(ExtensionAttribute), false) &&
                            !x.ContainsGenericParameters
                            ).ToList();

                    // Loop through the results.
                    foreach (var method in typeMethods)
                    {
                        // Get the parameter info.
                        var pi = method.GetParameters();

                        // Get the LHS of the comparison.
                        var lhs = pi.Select(x => x.ParameterType).ToArray();

                        // Get the RHS of the comparison.
                        var rhs = new Type[] { extensionType }.Concat(parameterTypes).ToArray();

                        // At this point we only know that we have an extension method with a matching
                        //   name. We don't know if the signatures match, or not. Let's go determine 
                        //   that now.

                        // Ignore methods with mismatched signatures.
                        if (lhs.Count() != rhs.Count())
                        {
                            // Skip it.
                            continue;
                        }

                        // Now verify that all the argument types either match, or at least, are
                        //   assignable types (so that inherited/derived types work).

                        var shouldAdd = true; // Assume we should add the method.

                        // Check for non-assignable args.
                        for (int z = 0; z < lhs.Length; z++)
                        {
                            // Are the arg types not assignable?
                            if (false == lhs[z].IsAssignableFrom(rhs[z]))
                            {
                                // Nope, we don't want this method.
                                shouldAdd = false;
                                break; // Take no for an answer.
                            }
                        }

                        // Should we add the method?
                        if (shouldAdd)
                        {
                            // See if we've already added the method.
                            shouldAdd |= methods.Any(x => x.Name == method.Name);
                        }

                        // Should we add the method?
                        if (shouldAdd)
                        {
                            // Add the method.
                            methods.Add(method);
                            break; // Take yes for an answer.
                        }
                    }
                }
            });

            // Return the results.
            return methods;
        }

        // *******************************************************************

        /// <summary>
        /// This method searches among the assemblies loaded into the current 
        /// app-domain for any public extension methods associated with the 
        /// specified type and signature.
        /// </summary>
        /// <typeparam name="T">The type of generic argument to match.</typeparam>
        /// <param name="appDomain">The application domain to use for the operation.</param>
        /// <param name="extensionType">The type to match against.</param>
        /// <param name="methodName">The method name to match against.</param>
        /// <param name="parameterTypes">An optional list of parameter type(s) 
        /// to match against.</param>
        /// <param name="assemblyWhiteList">An optional white list of assembly
        /// names - for narrowing the range of assemblies searched.</param>
        /// <param name="assemblyBlackList">An optional white list of assembly
        /// names - for narrowing the range of assemblies searched.</param>
        /// <returns>A sequence of <see cref="MethodInfo"/> objects representing
        /// zero or more matching extension methods.</returns>
        public static IEnumerable<MethodInfo> ExtensionMethods<T>(
            this AppDomain appDomain,
            Type extensionType,
            string methodName,
            Type[] parameterTypes = null,
            string assemblyWhiteList = "",
            string assemblyBlackList = "Microsoft*, System*, mscorlib, netstandard"
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(appDomain, nameof(appDomain))
                .ThrowIfNull(extensionType, nameof(extensionType))
                .ThrowIfNullOrEmpty(methodName, nameof(methodName));

            // Should we supply a valid value?
            if (parameterTypes == null)
            {
                parameterTypes = new Type[0];
            }

            // Get the list of currently loaded assemblies.
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

            // Was a white list specified?
            if (!string.IsNullOrEmpty(assemblyWhiteList))
            {
                // Look for assemblies in the white list we might need to load.
                var toLoad = assemblyWhiteList.Split(',').Where(
                    x => assemblies.Any(y => !y.GetName().Name.IsMatch(x))
                    );

                // Did we find any?
                if (toLoad.Any())
                {
                    // Loop and load any missing whitelisted assemblies.
                    toLoad.ForEach(x =>
                    {
                        // Look for matching files.
                        var files = Directory.GetFiles(
                            AppDomain.CurrentDomain.BaseDirectory,
                            x.EndsWith(".dll") ? x : $"{x}.dll"
                            );

                        // Loop through the files.
                        foreach (var file in files)
                        {
                            try
                            {
                                _ = Assembly.LoadFile(file);
                            }
                            catch (Exception)
                            {
                                // Don't care, just won't search this file.
                            }
                        }
                    });

                    // Get the list of currently loaded assemblies.
                    assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

                    // Filter out anything that doesn't belong.
                    assemblies = assemblies.ApplyWhiteList(
                        x => x.GetName().Name, assemblyWhiteList
                        ).ToList();
                }

                // At this point the assembly list should only contain assemblies 
                //   that are already loaded in the app-domain, and have their 
                //   name(s) on the white list.
            }

            // Was a black list specified?
            if (!string.IsNullOrEmpty(assemblyBlackList))
            {
                // Split the black list into parts.
                var blackParts = assemblyBlackList.Split(',');

                // Filter out anything that doesn't belong.
                assemblies = assemblies.ApplyBlackList(
                    x => x.GetName().Name, assemblyBlackList
                    ).ToList();

                // At this point the assembly list should only contains those
                //   assemblies that are already loaded in the app-domain and/or
                //   white listed, and are not on the black list.
            }

            // Now we have a list of assemblies that contains juuuuuust the right items
            //   and we can use that list to perform our next search.

            var methods = new List<MethodInfo>();

            // Create options for the parallel operation.
            var options = new ParallelOptions()
            {
#if DEBUG
                MaxDegreeOfParallelism = 1 // <-- to make debugging easier.
#else
                MaxDegreeOfParallelism = Environment.ProcessorCount
#endif
            };

            // We should be able to conduct the search in parallel.
            Parallel.ForEach(assemblies, options, (assembly) =>
            {
                // Look for types that are public, sealed and non-nested.
                var types = assembly.GetTypes().Where(x => 
                        x.IsClass && x.IsPublic &&
                        x.IsSealed && !x.IsNested
                        );
                                   
                // Loop through each matching type.
                foreach (var type in types)
                {
                    // Look for any public extension methods with a matching name
                    //   that contain generic type arguments.
                    var typeMethods = type.GetMethods(
                        BindingFlags.Static | BindingFlags.Public
                        ).Where(x => 
                            x.Name == methodName && 
                            x.IsDefined(typeof(ExtensionAttribute), false) &&
                            x.ContainsGenericParameters
                            );

                    // Loop through any methods we find.
                    foreach (var method in typeMethods)
                    {
                        // Get the generic type arguments.
                        var genArgs = method.GetGenericArguments();
                        
                        // Do the type args counts match?
                        if (1 != genArgs.Length)
                        {
                            continue; // Generic type counts don't match.
                        }

                        // Do the type args themselves match - or at least, are they
                        //   assignable?
                        if (false == genArgs[0].BaseType.IsAssignableFrom(typeof(T)))
                        {
                            continue; // Generic types don't match.
                        }

                        // Get the parameter info.
                        var pi = method.GetParameters();

                        // Get the LHS of the comparison.
                        var lhs = pi.Select(x => x.ParameterType).ToArray();

                        // Get the RHS of the comparison.
                        var rhs = new Type[] { extensionType }.Concat(parameterTypes).ToArray();

                        // At this point we only know that we have an extension method with a matching
                        //   name and type argument(s). We don't know if the signatures match, or not.
                        //   Let's go determine that now.

                        // Ignore methods with mismatched signatures.
                        if (lhs.Count() != rhs.Count())
                        {
                            // Skip it.
                            continue;
                        }

                        // Now verify that all the argument types either match, or at least, are
                        //   assignable types (so that inherited/derived types work).

                        var shouldAdd = true; // Assume we should add the method.

                        // Check for non-assignable args.
                        for (int z = 0; z < lhs.Length; z++)
                        {
                            // Are the arg types not assignable?
                            if (false == lhs[z].IsAssignableFrom(rhs[z]))
                            {
                                shouldAdd = false; // Nope, we don't want this method.
                                break;
                            }
                        }

                        // Should we add the method?
                        if (shouldAdd)
                        {
                            methods.Add(method); // Found a match.
                            break;
                        }
                    }
                }
            });

            // Return the results.
            return methods;
        }

        // *******************************************************************

        /// <summary>
        /// This method searches among the assemblies loaded into the current 
        /// app-domain for any public extension methods associated with the 
        /// specified type and signature.
        /// </summary>
        /// <typeparam name="T1">The first type argument to match.</typeparam>
        /// <typeparam name="T2">The second type argument to match.</typeparam>
        /// <param name="appDomain">The application domain to use for the operation.</param>
        /// <param name="extensionType">The type to match against.</param>
        /// <param name="methodName">The method name to match against.</param>
        /// <param name="parameterTypes">An optional list of parameter type(s) 
        /// to match against.</param>
        /// <param name="assemblyWhiteList">An optional white list of assembly
        /// names - for narrowing the range of assemblies searched.</param>
        /// <param name="assemblyBlackList">An optional white list of assembly
        /// names - for narrowing the range of assemblies searched.</param>
        /// <returns>A sequence of <see cref="MethodInfo"/> objects representing
        /// zero or more matching extension methods.</returns>
        public static IEnumerable<MethodInfo> ExtensionMethods<T1, T2>(
            this AppDomain appDomain,
            Type extensionType,
            string methodName,
            Type[] parameterTypes = null,
            string assemblyWhiteList = "",
            string assemblyBlackList = "Microsoft*, System*, mscorlib, netstandard"
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(appDomain, nameof(appDomain))
                .ThrowIfNull(extensionType, nameof(extensionType))
                .ThrowIfNullOrEmpty(methodName, nameof(methodName));

            // Should we supply a valid value?
            if (parameterTypes == null)
            {
                parameterTypes = new Type[0];
            }

            // Get the list of currently loaded assemblies.
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

            // Was a white list specified?
            if (!string.IsNullOrEmpty(assemblyWhiteList))
            {
                // Look for assemblies in the white list we might need to load.
                var toLoad = assemblyWhiteList.Split(',').Where(
                    x => assemblies.Any(y => !y.GetName().Name.IsMatch(x))
                    );

                // Did we find any?
                if (toLoad.Any())
                {
                    // Loop and load any missing whitelisted assemblies.
                    toLoad.ForEach(x =>
                    {
                        // Look for matching files.
                        var files = Directory.GetFiles(
                            AppDomain.CurrentDomain.BaseDirectory,
                            x.EndsWith(".dll") ? x : $"{x}.dll"
                            );

                        // Loop through the files.
                        foreach (var file in files)
                        {
                            try
                            {
                                _ = Assembly.LoadFile(file);
                            }
                            catch (Exception)
                            {
                                // Don't care, just won't search this file.
                            }
                        }
                    });

                    // Get the list of currently loaded assemblies.
                    assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

                    // Filter out anything that doesn't belong.
                    assemblies = assemblies.ApplyWhiteList(
                        x => x.GetName().Name, assemblyWhiteList
                        ).ToList();
                }

                // At this point the assembly list should only contain assemblies 
                //   that are already loaded in the app-domain, and have their 
                //   name(s) on the white list.
            }

            // Was a black list specified?
            if (!string.IsNullOrEmpty(assemblyBlackList))
            {
                // Split the black list into parts.
                var blackParts = assemblyBlackList.Split(',');

                // Filter out anything that doesn't belong.
                assemblies = assemblies.ApplyBlackList(
                    x => x.GetName().Name, assemblyBlackList
                    ).ToList();

                // At this point the assembly list should only contains those
                //   assemblies that are already loaded in the app-domain and/or
                //   white listed, and are not on the black list.
            }

            // Now we have a list of assemblies that contains juuuuuust the right items
            //   and we can use that list to perform our next search.

            var methods = new List<MethodInfo>();

            // Create options for the parallel operation.
            var options = new ParallelOptions()
            {
#if DEBUG
                MaxDegreeOfParallelism = 1 // <-- to make debugging easier.
#else
                MaxDegreeOfParallelism = Environment.ProcessorCount
#endif
            };

            // We should be able to conduct the search in parallel.
            Parallel.ForEach(assemblies, options, (assembly) =>
            {
                // Look for types that are public, sealed and non-nested.
                var types = assembly.GetTypes().Where(x =>
                        x.IsClass && x.IsPublic &&
                        x.IsSealed && !x.IsNested
                        );

                // Loop through each matching type.
                foreach (var type in types)
                {
                    // Look for any public extension methods with a matching name
                    //   that contain generic type arguments.
                    var typeMethods = type.GetMethods(
                        BindingFlags.Static | BindingFlags.Public
                        ).Where(x =>
                            x.Name == methodName &&
                            x.IsDefined(typeof(ExtensionAttribute), false) &&
                            x.ContainsGenericParameters
                            );

                    // Loop through any methods we find.
                    foreach (var method in typeMethods)
                    {
                        // Get the generic type arguments.
                        var genArgs = method.GetGenericArguments();

                        // Do the type args counts match?
                        if (2 != genArgs.Length)
                        {
                            continue; // Generic type counts don't match.
                        }

                        // Do the type args themselves match - or at least, are they
                        //   assignable?
                        if (false == genArgs[0].BaseType.IsAssignableFrom(typeof(T1)) ||
                            false == genArgs[1].BaseType.IsAssignableFrom(typeof(T2)))
                        {
                            continue; // Generic types don't match.
                        }

                        // Get the parameter info.
                        var pi = method.GetParameters();

                        // Get the LHS of the comparison.
                        var lhs = pi.Select(x => x.ParameterType).ToArray();

                        // Get the RHS of the comparison.
                        var rhs = new Type[] { extensionType }.Concat(parameterTypes).ToArray();

                        // At this point we only know that we have an extension method with a matching
                        //   name and type argument(s). We don't know if the signatures match, or not.
                        //   Let's go determine that now.

                        // Ignore methods with mismatched signatures.
                        if (lhs.Count() != rhs.Count())
                        {
                            // Skip it.
                            continue;
                        }

                        // Now verify that all the argument types either match, or at least, are
                        //   assignable types (so that inherited/derived types work).

                        var shouldAdd = true; // Assume we should add the method.

                        // Check for non-assignable args.
                        for (int z = 0; z < lhs.Length; z++)
                        {
                            // Are the arg types not assignable?
                            if (false == lhs[z].IsAssignableFrom(rhs[z]))
                            {
                                shouldAdd = false; // Nope, we don't want this method.
                                break;
                            }
                        }

                        // Should we add the method?
                        if (shouldAdd)
                        {
                            methods.Add(method); // Found a match.
                            break;
                        }
                    }
                }
            });

            // Return the results.
            return methods;
        }

        #endregion
    }
}
