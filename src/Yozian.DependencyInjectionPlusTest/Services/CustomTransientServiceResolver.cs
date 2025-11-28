using System;
using Microsoft.Extensions.DependencyInjection;
using Yozian.DependencyInjectionPlus;

namespace Yozian.DependencyInjectionPlusTest.Services;

public class CustomTransientServiceResolver : IServiceResolver
{
    public object Resolve(IServiceProvider provider, Type serviceType)
    {
        if (serviceType == typeof(MyTransientService))
        {
            return new CustomTransientService();
        }

        return ActivatorUtilities.CreateInstance(provider, serviceType);
    }
}
