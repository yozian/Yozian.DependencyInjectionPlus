using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Yozian.DependencyInjectionPlus;
using Yozian.DependencyInjectionPlusTest.Services;

namespace Yozian.DependencyInjectionPlusTest;

public class ServiceResolverEdgeCasesTests
{
    [SetUp]
    public void SetUp()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
    }

    [Test]
    public void RegisterServicesOfAssembly_RegistersTypesFromProvidedAssembly()
    {
        var collection = new ServiceCollection();

        collection.RegisterServicesOfAssembly(
            typeof(MySingletonService).Assembly,
            t => t == typeof(MySingletonService)
        );

        using var provider = collection.BuildServiceProvider();

        var singleton = provider.GetRequiredService<MySingletonService>();

        Assert.IsNotNull(singleton);
    }

    [Test]
    public void RegisterServices_WithDelegateAndInstanceResolvers_ThrowsArgumentException()
    {
        var collection = new ServiceCollection();

        Assert.Throws<ArgumentException>(() =>
            collection.RegisterServices(
                "Yozian.DependencyInjectionPlusTest",
                serviceResolver: (_, _) => new object(),
                serviceResolverInstance: new CustomTransientServiceResolver()
            )
        );
    }

    [Test]
    public void ServiceResolverReturningNull_ThrowsInvalidOperationException()
    {
        var collection = new ServiceCollection();

        collection.RegisterServices(
            "Yozian.DependencyInjectionPlusTest",
            t => t == typeof(MySingletonService),
            serviceResolver: (_, _) => null
        );

        using var provider = collection.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<MySingletonService>());
    }

    [Test]
    public void ServiceResolverReturningWrongType_ThrowsInvalidOperationException()
    {
        var collection = new ServiceCollection();

        collection.RegisterServices(
            "Yozian.DependencyInjectionPlusTest",
            t => t == typeof(MySingletonService),
            serviceResolver: (_, _) => new CustomTransientService()
        );

        using var provider = collection.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<MySingletonService>());
    }
}