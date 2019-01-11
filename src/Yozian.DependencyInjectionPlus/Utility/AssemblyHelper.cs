using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Yozian.DependencyInjectionPlus.Utility
{
    internal static class AssemblyHelper
    {
        public static IEnumerable<Type> GetAllTypesOfInterface<T>()
        {
            return GetAllExportedTypes()
                    .Where(t => typeof(T).IsAssignableFrom(t));
        }

        public static IEnumerable<Type> GetAllTypesOfInterface<T>(Assembly assembly)
        {
            return GetAllExportedTypes(assembly)
                    .Where(t => typeof(T).IsAssignableFrom(t));
        }

        public static IEnumerable<Type> GetAllTypesByAttribute<TAttribute>(string assemblyPrefixName)
                where TAttribute : Attribute
        {
            return GetAllExportedTypes(assemblyPrefixName)
                    .Where(t => t.IsDefined(typeof(TAttribute)));
        }

        public static IEnumerable<Type> GetAllTypesByBaseAttribute<TAttribute>(string assemblyPrefixName)
        where TAttribute : Attribute
        {
            return GetAllExportedTypes(assemblyPrefixName)
                    .Where(t => t.IsDefined(typeof(TAttribute)));
        }

        public static IEnumerable<Type> GetAllTypesByAttribute<TAttribute>(Assembly assembly)
               where TAttribute : Attribute
        {
            return GetAllExportedTypes(assembly)
                    .Where(t => t.IsDefined(typeof(TAttribute)));
        }

        /// <summary>
        /// auto filter out Microsoft and default .net objects
        /// </summary>
        /// <param name="assemblyPrefixName"></param>
        /// <returns></returns>
        private static IEnumerable<Type> GetAllExportedTypes(string assemblyPrefixName = "")
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            return assemblies
                 .Where(a => !a.FullName.StartsWith("Microsoft"))
                 .Where(a => !a.FullName.StartsWith("System"))
                 .Where(a =>
                 {
                     var matched = a.FullName.StartsWith(assemblyPrefixName);
                     return matched;
                 })
                 .SelectMany(a => a.ExportedTypes)
                 .ToList();
        }

        private static IEnumerable<Type> GetAllExportedTypes(Assembly assembly)
        {
            return assembly
              .ExportedTypes
              .ToList();
        }
    }
}