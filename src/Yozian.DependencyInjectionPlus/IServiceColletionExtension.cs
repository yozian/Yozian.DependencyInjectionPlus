﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yozian.DependencyInjectionPlus.Attributes;
using Yozian.DependencyInjectionPlus.Exceptions;
using Yozian.DependencyInjectionPlus.Utility;
using Yozian.Extension;

namespace Yozian.DependencyInjectionPlus
{
    public static class IServiceCollectionExtension
    {
        /// <summary>
        /// </summary>
        /// <param name="assemblyPrefix">filter out the matched assemblies</param>
        /// <param name="filter">determine which service type should be registered</param>
        /// <returns></returns>
        public static IServiceCollection RegisterServices(
            this IServiceCollection @this,
            string assemblyPrefix = "",
            Func<Type, bool> filter = null,
            ILogger logger = null
        )
        {
            var types = AssemblyHelper.GetAllTypesByBaseAttribute<ServiceAttribute>(assemblyPrefix)
                .AsQueryable()
                .WhereWhen(null != filter, t => filter(t))
                .AsEnumerable();

            RegisterTypes(@this, types, logger);

            return @this;
        }

        /// <summary>
        /// scan services with di attributes in the assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="filter">determine which service type should be registered</param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterServicesOfAssembly(
            this IServiceCollection @this,
            Assembly assembly,
            Func<Type, bool> filter = null,
            ILogger logger = null
        )
        {
            var types = AssemblyHelper.GetAllTypesByAttribute<ServiceAttribute>(assembly)
                .AsQueryable()
                .WhereWhen(null != filter, t => filter(t))
                .AsEnumerable();

            RegisterTypes(@this, types, logger);

            return @this;
        }

        private static void RegisterTypes(IServiceCollection container, IEnumerable<Type> types, ILogger logger)
        {
            var notSpecifyEnv = "not-specified-env";
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? notSpecifyEnv;

            logger?.LogInformation($"DI Process Env: {env}");

            types
                .Select(
                    t =>
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
                            attr?.DiScope,
                            Interfaces = attr?.ServiceTypes,
                            IsActive = isActive
                        };
                    }
                )
                .Where(x => x.IsActive)
                .GroupBy(x => x.DiScope)
                .OrderBy(x => x.Key)
                .ForEach(
                    g =>
                    {
                        logger?.LogInformation($"Register {g.Key} Services , Total: {g.Count()}");

                        logger?.LogInformation("\t [ConcreteType : ServiceTypes]");

                        g.ForEach(
                            (item, num) =>
                            {
                                // register for concrete type
                                switch (g.Key)
                                {
                                    case DiScope.Transient:
                                        container.AddTransient(item.ServiceImplementType, item.ServiceImplementType);

                                        break;

                                    case DiScope.Scoped:
                                        container.AddScoped(item.ServiceImplementType, item.ServiceImplementType);

                                        break;

                                    case DiScope.Singleton:
                                        container.AddSingleton(item.ServiceImplementType, item.ServiceImplementType);

                                        break;
                                }

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
                                item.Interfaces?.ForEach(
                                    interfaceType =>
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
                                        item.ServiceImplementType.Name
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
    }
}
