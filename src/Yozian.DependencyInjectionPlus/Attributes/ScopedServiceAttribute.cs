using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yozian.DependencyInjectionPlus.Attributes
{
    /// <summary>
    /// This attributes is mark the service to Scoped
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ScopedServiceAttribute : ServiceAttribute
    {
        public ScopedServiceAttribute(Type serviceType = null)
            : this()
        {
            this.ServiceTypes = new List<Type> { serviceType };
        }

        public ScopedServiceAttribute(params Type[] serviceType)
            : this()
        {
            this.ServiceTypes = serviceType;
        }

        public ScopedServiceAttribute()
        {
            this.DiScope = DiScope.Scoped;
        }
    }
}