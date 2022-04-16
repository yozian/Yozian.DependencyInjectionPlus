using System;
using System.Collections.Generic;

namespace Yozian.DependencyInjectionPlus.Attributes
{
    /// <summary>
    /// This attributes is mark the service to Scoped
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ScopedServiceAttribute : ServiceAttribute
    {
        public ScopedServiceAttribute(Type serviceType = null)
            : this()
        {
            this.ServiceTypes = new List<Type>
            {
                serviceType
            };
        }

        public ScopedServiceAttribute(params Type[] serviceType)
            : this()
        {
            this.ServiceTypes = serviceType;
        }

        public ScopedServiceAttribute(string activeEnvs, params Type[] serviceType)
            : this(serviceType)
        {
            this.ActiveEnvs = activeEnvs;
        }

        public ScopedServiceAttribute(string activeEnvs)
            : this()
        {
            this.ActiveEnvs = activeEnvs;
        }

        public ScopedServiceAttribute()
        {
            this.DiScope = DiScope.Scoped;
        }
    }
}
