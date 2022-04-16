using System;
using System.Collections.Generic;

namespace Yozian.DependencyInjectionPlus.Attributes
{
    public abstract class ServiceAttribute : Attribute
    {
        internal DiScope DiScope { get; set; }

        public IEnumerable<Type> ServiceTypes { get; protected set; }

        public string ActiveEnvs { get; protected set; }
    }
}
