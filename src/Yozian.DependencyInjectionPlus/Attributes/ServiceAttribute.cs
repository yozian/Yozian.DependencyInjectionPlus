using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yozian.DependencyInjectionPlus.Attributes
{
    public abstract class ServiceAttribute : Attribute
    {
        internal DiScope DiScope { get; set; }

        public IEnumerable<Type> ServiceTypes { get; protected set; }

        public string ActiveEnvs { get; protected set; }
    }
}