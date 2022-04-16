using System;
using System.Collections.Generic;

namespace Yozian.DependencyInjectionPlus.Attributes
{
    /// <summary>
    /// This attributes is mark the service to Singleton
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SingletonServiceAttribute : ServiceAttribute
    {
        public SingletonServiceAttribute(Type serviceType = null)
            : this()
        {
            this.ServiceTypes = new List<Type>
            {
                serviceType
            };
        }

        public SingletonServiceAttribute(params Type[] serviceType)
            : this()
        {
            this.ServiceTypes = serviceType;
        }

        public SingletonServiceAttribute(string activeEnvs, params Type[] serviceType)
            : this(serviceType)
        {
            this.ActiveEnvs = activeEnvs;
        }

        public SingletonServiceAttribute(string activeEnvs)
            : this()
        {
            this.ActiveEnvs = activeEnvs;
        }

        public SingletonServiceAttribute()
        {
            this.DiScope = DiScope.Singleton;
        }
    }
}
