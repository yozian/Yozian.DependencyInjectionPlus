using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yozian.DependencyInjectionPlus.Attributes;
using Yozian.DependencyInjectionPlus.Exceptions;
using Yozian.DependencyInjectionPlus.Utility;
using Yozian.Extension;

namespace Yozian.DependencyInjectionPlus;

public static class IServiceCollectionExtension
{
    /// <summary>
    /// </summary>
    /// <param name="assemblyPrefix">filter out the matched assemblies</param>
    /// <param name="filter">determine which service type should be registered</param>
    /// <param name="logger">optional logger for diagnostics</param>
    /// <param name="serviceResolver">custom resolver for the concrete service instance</param>
    /// <param name="serviceResolverInstance">instance that implements IServiceResolver for custom instantiation</param>
    /// <returns></returns>
    public static IServiceCollection RegisterServices(
        this IServiceCollection @this,
        string assemblyPrefix = "",
        Func<Type, bool> filter = null,
        ILogger logger = null,
        Func<IServiceProvider, Type, object> serviceResolver = null,
        IServiceResolver serviceResolverInstance = null
    )
    {
        var types = AssemblyHelper.GetAllTypesByBaseAttribute<ServiceAttribute>(assemblyPrefix)
            .AsQueryable()
            .WhereWhen(null != filter, t => filter(t))
            .AsEnumerable();

        RegisterTypes(
            @this,
            types,
            logger,
            serviceResolver,
            serviceResolverInstance
        );

        return @this;
    }

    /// <summary>
    /// scan services with di attributes in the assembly
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="filter">determine which service type should be registered</param>
    /// <param name="logger"></param>
    /// <param name="serviceResolver">custom resolver for the concrete service instance</param>
    /// <param name="serviceResolverInstance">instance that implements IServiceResolver for custom instantiation</param>
    /// <returns></returns>
    public static IServiceCollection RegisterServicesOfAssembly(
        this IServiceCollection @this,
        Assembly assembly,
        Func<Type, bool> filter = null,
        ILogger logger = null,
        Func<IServiceProvider, Type, object> serviceResolver = null,
        IServiceResolver serviceResolverInstance = null
    )
    {
        var types = AssemblyHelper.GetAllTypesByAttribute<ServiceAttribute>(assembly)
            .AsQueryable()
            .WhereWhen(null != filter, t => filter(t))
            .AsEnumerable();

        RegisterTypes(
            @this,
            types,
            logger,
            serviceResolver,
            serviceResolverInstance
        );

        return @this;
    }

    private static void RegisterTypes(
        IServiceCollection container,
        IEnumerable<Type> types,
        ILogger logger,
        Func<IServiceProvider, Type, object> serviceResolver,
        IServiceResolver serviceResolverInstance
    )
    {
        var notSpecifyEnv = "not-specified-env";
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? notSpecifyEnv;

        logger?.LogInformation($"DI Process Env: {env}");

        var resolver = ComposeServiceResolver(serviceResolver, serviceResolverInstance);

        types
            .Select(t =>
                {
                    var attrType = t.CustomAttributes
                        .First(x => x.AttributeType.IsSubclassOf(typeof(ServiceAttribute)))
                        .AttributeType;

                    var attr = t.GetCustomAttribute(attrType, false) as ServiceAttribute;

                    var targetEnvs = attr?.ActiveEnvs
                        .SafeToString()
                        .Split(',')
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToList();

                    // available for non-specified env
                    var isActive = notSpecifyEnv.Equals(env);

                    if (!string.IsNullOrEmpty(env)
                        && !notSpecifyEnv.Equals(env)
                        && targetEnvs?.Count > 0)
                    {
                        isActive = targetEnvs.Contains(env);
                    }
                    else
                    {
                        // those no target env should be registered
                        isActive = true;
                    }

                    return new
                    {
                        ServiceImplementType = t,
                        DiAttribute = attrType,
                        attr.DiScope,
                        Interfaces = attr?.ServiceTypes,
                        IsActive = isActive,
                    };
                }
            )
            .Where(x => x.IsActive)
            .GroupBy(x => x.DiScope)
            .OrderBy(x => x.Key)
            .ForEach(g =>
                {
                    logger?.LogInformation($"Register {g.Key} Services , Total: {g.Count()}");

                    logger?.LogInformation("\t [ConcreteType : ServiceTypes]");

                    g.ForEach((item, num) =>
                        {
                            // register for concrete type
                            RegisterServiceImplementType(
                                container,
                                g.Key,
                                item.ServiceImplementType,
                                resolver
                            );

                            // make sure all provided interface are implemented by the service!
                            var implementedInterfaces = item.ServiceImplementType
                                .GetInterfaces()
                                .Select(i => i.FullName)
                                .ToList();

                            var notMatchedInterfaces = item.Interfaces?
                                .Where(i => !implementedInterfaces.Contains(i.FullName))
                                .ToList();

                            if (null != notMatchedInterfaces && notMatchedInterfaces.Count > 0)
                            {
                                throw new NonImplementedInterfaceProvidedException(
                                    $"{item.ServiceImplementType.Name} has NOT implement of [{
                                        string.Join(",", notMatchedInterfaces)}] but provide in {
                                            item.DiAttribute.Name
                                        } "
                                );
                            }

                            // register for interfaces
                            item.Interfaces?.ForEach(interfaceType =>
                                {
                                    // we should use this to make sure
                                    // that injection either by the class type or by the interfaces
                                    // are refer to the same instance
                                    switch (g.Key)
                                    {
                                        case DiScope.Transient:
                                            container.AddTransient(
                                                interfaceType,
                                                p => p.GetRequiredService(item.ServiceImplementType)
                                            );

                                            break;

                                        case DiScope.Scoped:
                                            container.AddScoped(
                                                interfaceType,
                                                p => p.GetRequiredService(item.ServiceImplementType)
                                            );

                                            break;

                                        case DiScope.Singleton:
                                            container.AddSingleton(
                                                interfaceType,
                                                p => p.GetRequiredService(item.ServiceImplementType)
                                            );

                                            break;
                                    }
                                }
                            );

                            var registeredTypes = new List<string>
                                {
                                    item.ServiceImplementType.Name,
                                }
                                .Concat((item.Interfaces ?? new List<Type>()).Select(i => i.Name))
                                .ToList();

                            logger?.LogInformation(
                                $"\t ({num + 1}) {item.ServiceImplementType.Name}: {
                                    string.Join(", ", registeredTypes)
                                }"
                            );
                        }
                    );
                }
            );
    }

    private static Func<IServiceProvider, Type, object> ComposeServiceResolver(
        Func<IServiceProvider, Type, object> resolverDelegate,
        IServiceResolver serviceResolverInstance
    )
    {
        if (resolverDelegate != null && serviceResolverInstance != null)
        {
            throw new ArgumentException("Provide either serviceResolver delegate or IServiceResolver instance, not both.");
        }

        if (serviceResolverInstance == null)
        {
            return resolverDelegate;
        }

        return (provider, serviceType) => serviceResolverInstance.Resolve(provider, serviceType);
    }

    private static void RegisterServiceImplementType(
        IServiceCollection container,
        DiScope scope,
        Type serviceType,
        Func<IServiceProvider, Type, object> serviceResolver
    )
    {
        if (null == serviceResolver)
        {
            switch (scope)
            {
                case DiScope.Transient:
                    container.AddTransient(serviceType, serviceType);

                    return;

                case DiScope.Scoped:
                    container.AddScoped(serviceType, serviceType);

                    return;

                case DiScope.Singleton:
                    container.AddSingleton(serviceType, serviceType);

                    return;
            }
        }

        var factory = BuildResolverFactory(serviceType, serviceResolver);

        switch (scope)
        {
            case DiScope.Transient:
                container.AddTransient(serviceType, factory);

                break;

            case DiScope.Scoped:
                container.AddScoped(serviceType, factory);

                break;

            case DiScope.Singleton:
                container.AddSingleton(serviceType, factory);

                break;
        }
    }

    private static Func<IServiceProvider, object> BuildResolverFactory(
        Type serviceType,
        Func<IServiceProvider, Type, object> serviceResolver
    )
    {
        return provider =>
        {
            var resolved = serviceResolver(provider, serviceType);

            if (null == resolved)
            {
                throw new InvalidOperationException(
                    $"serviceResolver should not return null for {serviceType.FullName}"
                );
            }

            if (!serviceType.IsInstanceOfType(resolved))
            {
                throw new InvalidOperationException(
                    $"serviceResolver result {resolved.GetType().FullName} is not assignable to {serviceType.FullName}"
                );
            }

            return resolved;
        };
    }
}
