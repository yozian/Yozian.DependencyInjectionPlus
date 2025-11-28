using System;

namespace Yozian.DependencyInjectionPlus;

public interface IServiceResolver
{
    object Resolve(IServiceProvider provider, Type serviceType);
}
