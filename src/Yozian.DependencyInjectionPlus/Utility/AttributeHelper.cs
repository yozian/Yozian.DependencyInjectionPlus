using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Yozian.DependencyInjectionPlus.Utility
{
    internal static class AttributeHelper
    {
        /// <summary>
        /// Gets an attribute value on an instance's field
        /// </summary>
        public static TAttr GetAttribute<T, TAttr>(this T @this)
            where TAttr : Attribute
        {
            return @this.GetType()
               .GetMember(@this.ToString())
               .First()
               .GetCustomAttributes(typeof(TAttr), false)
               .First() as TAttr;
        }

        public static IEnumerable<PropertyInfo> GetPropertyInfoByAttr<T>(this object @this)
            where T : Attribute
        {
            var props = @this.GetType()
               .GetProperties()
               .Where(
                    prop => Attribute.IsDefined(prop, typeof(T))
                );

            return props;
        }

        public static IEnumerable<PropertyInfo> GetPropertyInfoByAttr(
            this object @this,
            Type attrType
        )
        {
            var props = @this.GetType()
               .GetProperties()
               .Where(
                    prop => Attribute.IsDefined(prop, attrType)
                );

            return props;
        }
    }
}
