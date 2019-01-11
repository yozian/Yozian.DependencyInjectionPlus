using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yozian.DependencyInjectionPlus.Attributes
{
    /// <summary>
    /// This attributes is mark the service to Transient
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TransientServiceAttribute : ServiceAttribute
    {
        public TransientServiceAttribute(Type serviceType = null)
            : this()
        {
            this.ServiceTypes = new List<Type> { serviceType };
        }

        public TransientServiceAttribute(params Type[] serviceType)
            : this()
        {
            this.ServiceTypes = serviceType;
        }

        public TransientServiceAttribute()
        {
            this.DiScope = DiScope.Transient;
        }
    }
}