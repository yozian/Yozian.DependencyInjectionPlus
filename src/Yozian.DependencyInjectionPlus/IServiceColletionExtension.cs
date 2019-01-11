using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Yozian.DependencyInjectionPlus.Attributes;
using Yozian.DependencyInjectionPlus.Exceptions;
using Yozian.DependencyInjectionPlus.Utility;
using Yozian.Extension;

namespace Yozian.DependencyInjectionPlus
{
    public static class IServiceColletionExtension
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="this"></param>
        /// <param name="assemblyPrefix">filter out the matched assemblies</param>
        /// <param name="filter">determin which service type should be regitered</param>
        /// <returns></returns>
        public static IServiceCollection RegisterServices(
           this IServiceCollection @this,
           string assemblyPrefix = "",
           Func<Type, bool> filter = null
           )
        {
            var types = AssemblyHelper.GetAllTypesByBaseAttribute<ServiceAttribute>(assemblyPrefix)
                  .AsQueryable()
                  .WhereWhen(null != filter, t => filter(t))
                  .AsEnumerable();

            registerTypes(@this, types);

            return @this;
        }

        public static IServiceCollection RegisterServicesOfAssembly(
           this IServiceCollection @this,
           Assembly assembly,
           Func<Type, bool> filter = null
           )
        {
            var types = AssemblyHelper.GetAllTypesByAttribute<ServiceAttribute>(assembly)
                  .AsQueryable()
                  .WhereWhen(null != filter, t => filter(t))
                  .AsEnumerable();

            registerTypes(@this, types);

            return @this;
        }

        private static void registerTypes(IServiceCollection container, IEnumerable<Type> types)
        {
            types
            .Select(t =>
             {
                 var attrType = t.CustomAttributes
                   .First(x => x.AttributeType.IsSubclassOf(typeof(ServiceAttribute)))
                   .AttributeType;

                 var attr = t.GetCustomAttribute(attrType, false) as ServiceAttribute;

                 return new
                 {
                     ServiceImplementType = t,
                     DiAttribute = attrType,
                     DiScope = attr.DiScope,
                     Interfaces = attr?.ServiceTypes
                 };
             })
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

                Console.WriteLine($"Register {g.Key} Services , Total: {g.Count()}");

                Console.WriteLine($"\t [ConcretType : SerivceTypes]");

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

                    Console.WriteLine($"\t ({(num + 1)}) {x.ServiceImplementType.Name}: {string.Join(", ", reigsterTypes)}");
                });
            });
        }
    }
}