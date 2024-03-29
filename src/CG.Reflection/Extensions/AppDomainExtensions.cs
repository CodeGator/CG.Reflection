﻿
namespace System;

/// <summary>
/// This class contains extension methods related to the <see cref="AppDomain"/>
/// type, for registering types related to reflection.
/// </summary>
public static partial class AppDomainExtensions
{
    // *******************************************************************
    // Public methods.
    // *******************************************************************

    #region Public methods

    /// <summary>
    /// This method searches among the assemblies loaded into the current 
    /// app-domain for any matching concrete types that are assignable to 
    /// the given type: <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to use for the search.</typeparam>
    /// <param name="appDomain">The application domain to use for the operation.</param>
    /// <param name="assemblyWhiteList">An optional white list of assembly
    /// names - for narrowing the range of assemblies searched.</param>
    /// <param name="assemblyBlackList">An optional white list of assembly
    /// names - for narrowing the range of assemblies searched.</param>
    /// <returns>A sequence of <see cref="Type"/> objects representing
    /// zero or more matching concrete types.</returns>
    public static IEnumerable<Type> FindConcreteTypes<T>(
        this AppDomain appDomain,
        string assemblyWhiteList = "",
        string assemblyBlackList = "Microsoft*, System*, mscorlib, netstandard"
        ) where T : class
    {

        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(appDomain, nameof(appDomain));

        // Get the type we'll use in our search.
        var searchType = typeof(T);

        // Get the list of currently loaded assemblies.
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

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
                // Loop and load any missing white listed assemblies.
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
                            Assembly.LoadFile(file);
                        }
                        catch (Exception)
                        {
                            // Don't care, just won't search this file.
                        }
                    }
                });

                // Get the list of currently loaded assemblies.
                assemblies = AppDomain.CurrentDomain
                    .GetAssemblies();

                // Filter out anything that doesn't belong.
                assemblies = assemblies.ApplyWhiteList(
                    x => x.GetName().Name,
                    assemblyWhiteList
                    ).ToArray();
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
                x => x.GetName().Name,
                assemblyBlackList
                ).ToArray();

            // At this point the assembly list should only contains those
            //   assemblies that are already loaded in the app-domain and/or
            //   white listed, and are not on the black list.
        }

        // Now we have a list of assemblies that contains juuuuuust the right items
        //   and we can use that list to perform our next search.

        var concreteTypes = new List<Type>();

        // Loop and search.
        foreach (var assembly in assemblies)
        {
            // Look for types that are public and non-abstract.
            var types = assembly.GetTypes().Where(x =>
                x.IsClass && !x.IsAbstract
                );

            // Loop through each matching type.
            foreach (var type in types)
            {
                if (type.IsAssignableTo(searchType))
                {
                    concreteTypes.Add(type);
                }
            }
        }

        // Remove any duplicates.
        var distinctList = concreteTypes.Distinct().ToList();

        // Return the results.
        return distinctList;
    }

    // *******************************************************************

    /// <summary>
    /// This method searches among those assemblies currently loaded into 
    /// the app-domain for any public extension methods with the given name, 
    /// that contain the specified parameter types.
    /// </summary>
    /// <param name="appDomain">The application domain to use for the operation.</param>
    /// <param name="extensionType">The type that is extended by the target 
    /// extension method.</param>
    /// <param name="extensionMethodName">The name of the target extension method.</param>
    /// <param name="parameterTypes">An optional list of parameter type(s) 
    /// to match against.</param>
    /// <param name="assemblyWhiteList">An optional white list of assembly
    /// names - for narrowing the range of assemblies searched.</param>
    /// <param name="assemblyBlackList">An optional white list of assembly
    /// names - for narrowing the range of assemblies searched.</param>
    /// <returns>A sequence of <see cref="MethodInfo"/> objects representing
    /// zero or more matching extension methods.</returns>
    /// <remarks>
    /// <para>
    /// If an assembly name is added to the <paramref name="assemblyWhiteList"/>
    /// parameter, but isn't currently loaded into the app-domain, then that
    /// assembly is loaded by this method prior to searching for a matching
    /// extension method.
    /// </para>
    /// </remarks>
    public static IEnumerable<MethodInfo> ExtensionMethods(
        this AppDomain appDomain,
        Type extensionType,
        string extensionMethodName,
        Type[]? parameterTypes = null,
        string assemblyWhiteList = "",
        string assemblyBlackList = "Microsoft*, System*, mscorlib, netstandard"
        )
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(appDomain, nameof(appDomain))
            .ThrowIfNull(extensionType, nameof(extensionType))
            .ThrowIfNullOrEmpty(extensionMethodName, nameof(extensionMethodName));

        // Should we supply a valid value?
        if (parameterTypes == null)
        {
            parameterTypes = new Type[0];
        }

        // Get the list of currently loaded assemblies.
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

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
                // Loop and load any missing white listed assemblies.
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
                            Assembly.LoadFile(file);
                        }
                        catch (Exception)
                        {
                            // Don't care, just won't search this file.
                        }
                    }
                });

                // Get the list of currently loaded assemblies.
                assemblies = AppDomain.CurrentDomain
                    .GetAssemblies();

                // Filter out anything that doesn't belong.
                assemblies = assemblies.ApplyWhiteList(
                    x => x.GetName().Name,
                    assemblyWhiteList
                    ).ToArray();
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
                x => x.GetName().Name,
                assemblyBlackList
                ).ToArray();

            // At this point the assembly list should only contains those
            //   assemblies that are already loaded in the app-domain and/or
            //   white listed, and are not on the black list.
        }

        // Now we have a list of assemblies that contains juuuuuust the right items
        //   and we can use that list to perform our next search.

        var methods = new List<MethodInfo>();

        // Loop and search.
        foreach (var assembly in assemblies)
        {
            // Look for types that are public, sealed, non-nested, non-generic classes.
            var types = assembly.GetTypes().Where(x =>
                x.IsClass && x.IsPublic &&
                x.IsSealed && !x.IsNested &&
                !x.IsGenericType
                );

            // Loop through each matching type.
            foreach (var type in types)
            {
                // Look for any public extension methods with a matching name that
                //   doesn't have generic type arguments.
                var typeMethods = type.GetMethods(
                    BindingFlags.Static |
                    BindingFlags.Public
                    ).Where(x =>
                        x.Name == extensionMethodName &&
                        x.IsDefined(typeof(ExtensionAttribute), false) &&
                        !x.ContainsGenericParameters
                        );

                // Loop through the results.
                foreach (var method in typeMethods)
                {
                    // Were parameter types provided?
                    if (parameterTypes.Any())
                    {
                        // If we get here then the caller specified parameter types to
                        //   match against, so we'll do that now.

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
                            if (false == lhs[z].IsAssignableTo(rhs[z]))
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
                    else
                    {
                        // If we get here then the caller didn't specify any
                        //   parameter types to match against, so, we'll take
                        //   all the methods we found and let the caller decide
                        //   which (if any) to use.

                        // Add the method.
                        methods.Add(method);
                    }
                }
            }
        }

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
    /// <param name="extensionType">The type that is extended by the target 
    /// extension method.</param>
    /// <param name="extensionMethodName">The name of the target extension method.</param>
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
        string extensionMethodName,
        Type[]? parameterTypes = null,
        string assemblyWhiteList = "",
        string assemblyBlackList = "Microsoft*, System*, mscorlib, netstandard"
        )
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(appDomain, nameof(appDomain))
            .ThrowIfNull(extensionType, nameof(extensionType))
            .ThrowIfNullOrEmpty(extensionMethodName, nameof(extensionMethodName));

        // Should we supply a valid value?
        if (parameterTypes == null)
        {
            parameterTypes = new Type[0];
        }

        // Get the list of currently loaded assemblies.
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

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
                assemblies = AppDomain.CurrentDomain.GetAssemblies();

                // Filter out anything that doesn't belong.
                assemblies = assemblies.ApplyWhiteList(
                    x => x.GetName().Name, assemblyWhiteList
                    ).ToArray();
            }

            // At this point the assembly list should only contain assemblies 
            //   that are already loaded in the app-domain, and have their 
            //   name(s) on the white list.
        }

        // Was a black list specified?
        if (!string.IsNullOrEmpty(assemblyBlackList))
        {
            // Split the black list into parts.
            var blackParts = assemblyBlackList.Split(',').ToArray();

            // Filter out anything that doesn't belong.
            assemblies = assemblies.ApplyBlackList(
                x => x.GetName().Name, assemblyBlackList
                ).ToArray();

            // At this point the assembly list should only contains those
            //   assemblies that are already loaded in the app-domain and/or
            //   white listed, and are not on the black list.
        }

        // Now we have a list of assemblies that contains juuuuuust the right items
        //   and we can use that list to perform our next search.

        var methods = new List<MethodInfo>();

        // Loop and search.
        foreach (var assembly in assemblies)
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
                        x.Name == extensionMethodName &&
                        x.IsDefined(typeof(ExtensionAttribute), false) &&
                        x.ContainsGenericParameters
                        );

                // Loop through any methods we find.
                foreach (var method in typeMethods)
                {
                    // Were parameter types provided?
                    if (parameterTypes.Any())
                    {
                        // If we get here then the caller specified parameter types to
                        //   match against, so we'll do that now.

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
                            if (false == lhs[z].IsAssignableTo(rhs[z]))
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
                    else
                    {
                        // If we get here then the caller didn't specify any
                        //   parameter types to match against, so, we'll take
                        //   all the methods we found and let the caller decide
                        //   which (if any) to use.

                        // Add the method.
                        methods.Add(method);
                    }
                }
            }
        }

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
    /// <param name="extensionType">The type that is extended by the target 
    /// extension method.</param>
    /// <param name="extensionMethodName">The name of the target extension method.</param>
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
        string extensionMethodName,
        Type[]? parameterTypes = null,
        string assemblyWhiteList = "",
        string assemblyBlackList = "Microsoft*, System*, mscorlib, netstandard"
        )
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(appDomain, nameof(appDomain))
            .ThrowIfNull(extensionType, nameof(extensionType))
            .ThrowIfNullOrEmpty(extensionMethodName, nameof(extensionMethodName));

        // Should we supply a valid value?
        if (parameterTypes == null)
        {
            parameterTypes = new Type[0];
        }

        // Get the list of currently loaded assemblies.
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

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
                assemblies = AppDomain.CurrentDomain.GetAssemblies();

                // Filter out anything that doesn't belong.
                assemblies = assemblies.ApplyWhiteList(
                    x => x.GetName().Name,
                    assemblyWhiteList
                    ).ToArray();
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
                x => x.GetName().Name,
                assemblyBlackList
                ).ToArray();

            // At this point the assembly list should only contains those
            //   assemblies that are already loaded in the app-domain and/or
            //   white listed, and are not on the black list.
        }

        // Now we have a list of assemblies that contains juuuuuust the right items
        //   and we can use that list to perform our next search.

        var methods = new List<MethodInfo>();

        // Loop and search.
        foreach (var assembly in assemblies)
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
                        x.Name == extensionMethodName &&
                        x.IsDefined(typeof(ExtensionAttribute), false) &&
                        x.ContainsGenericParameters
                        );

                // Loop through any methods we find.
                foreach (var method in typeMethods)
                {
                    // Were parameter types provided?
                    if (parameterTypes.Any())
                    {
                        // If we get here then the caller specified parameter types to
                        //   match against, so we'll do that now.

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
                            if (false == lhs[z].IsAssignableTo(rhs[z]))
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
                    else
                    {
                        // If we get here then the caller didn't specify any
                        //   parameter types to match against, so, we'll take
                        //   all the methods we found and let the caller decide
                        //   which (if any) to use.

                        // Add the method.
                        methods.Add(method);
                    }
                }
            }
        }

        // Return the results.
        return methods;
    }

    #endregion
}
