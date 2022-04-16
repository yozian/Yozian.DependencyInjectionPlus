using System;
using System.Collections.Generic;

namespace Yozian.DependencyInjectionPlus.Attributes
{
    /// <summary>
    /// This attributes is mark the service to Transient
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class TransientServiceAttribute : ServiceAttribute
    {
        public TransientServiceAttribute(Type serviceType = null)
            : this()
        {
            this.ServiceTypes = new List<Type>
            {
                serviceType
            };
        }

        public TransientServiceAttribute(params Type[] serviceType)
            : this()
        {
            this.ServiceTypes = serviceType;
        }

        public TransientServiceAttribute(string activeEnvs, params Type[] serviceType)
            : this(serviceType)
        {
            this.ActiveEnvs = activeEnvs;
        }

        public TransientServiceAttribute(string activeEnvs)
            : this()
        {
            this.ActiveEnvs = activeEnvs;
        }

        public TransientServiceAttribute()
        {
            this.DiScope = DiScope.Transient;
        }
    }
}
