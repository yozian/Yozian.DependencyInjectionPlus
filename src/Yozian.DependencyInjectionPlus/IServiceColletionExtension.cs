using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        ///
        /// </summary>
        /// <param name="assemblyPrefix">filter out the matched assemblies</param>
        /// <param name="filter">determin which service type should be regitered</param>
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

            registerTypes(@this, types, logger);

            return @this;
        }

        /// <summary>
        ///  scan services with di attributes in the assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="filter">determine which service type should be regitered</param>
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

            registerTypes(@this, types, logger);

            return @this;
        }

        private static void registerTypes(IServiceCollection container, IEnumerable<Type> types, ILogger logger)
        {
            var notSpecifyEnv = "not-specified-env";
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? notSpecifyEnv;

            logger?.LogInformation($"DI Process Env: {env}");

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
                     DiScope = attr?.DiScope,
                     Interfaces = attr?.ServiceTypes,
                     IsActive = isActive
                 };
             })
            .Where(x => x.IsActive)
            .GroupBy(x => x.DiScope)
            .OrderBy(x => x.Key)
            .ForEach(g =>
            {
                Func<Type, Type, IServiceCollection> register = null;

                switch (g.Key)
                {
                    case DiScope.Transient:
                        register = container.AddTransient;
                        break;

                    case DiScope.Scoped:
                        register = container.AddScoped;
                        break;

                    case DiScope.Singleton:
                        register = container.AddSingleton;
                        break;
                }

                logger?.LogInformation($"Register {g.Key} Services , Total: {g.Count()}");

                logger?.LogInformation($"\t [ConcretType : SerivceTypes]");

                g.ForEach((x, num) =>
                {
                    // register for concret type
                    register(x.ServiceImplementType, x.ServiceImplementType);

                    // make sure all provided interface are implemented by the service!
                    var implementedInterfaces = x.ServiceImplementType
                        .GetInterfaces()
                        .Select(i => i.FullName)
                        .ToList();

                    var notMatchedInterfaces = x.Interfaces?
                        .Where(i => !implementedInterfaces.Contains(i.FullName))
                        .ToList();

                    if (null != notMatchedInterfaces && notMatchedInterfaces.Count > 0)
                    {
                        throw new NonImplementedInterfaceProvidedException($"{x.ServiceImplementType.Name} has NOT implement of [{string.Join(",", notMatchedInterfaces)}] but provied in {x.DiAttribute.Name} ");
                    }

                    // register for interfaces
                    x.Interfaces?.ForEach(i => register(i, x.ServiceImplementType));

                    var reigsterTypes = new List<string>() {
                        x.ServiceImplementType.Name
                    }
                    .Concat((x.Interfaces ?? new List<Type>()).Select(i => i.Name))
                    .ToList();

                    logger?.LogInformation($"\t ({(num + 1)}) {x.ServiceImplementType.Name}: {string.Join(", ", reigsterTypes)}");
                });
            });
        }
    }
}
