using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Yozian.DependencyInjectionPlus;
using Yozian.DependencyInjectionPlus.Attributes;
using Yozian.DependencyInjectionPlus.Utility;
using Yozian.DependencyInjectionPlusTest.Services;

namespace Yozian.DependencyInjectionPlusTest;

public class AttributeAndUtilityTests
{
    [Test]
    public void TransientAttribute_DefaultScopeAndNoServiceTypes()
    {
        var attr = new TransientServiceAttribute();

        Assert.AreEqual(DiScope.Transient, GetScope(attr));
        Assert.IsNull(attr.ServiceTypes);
        Assert.IsNull(attr.ActiveEnvs);
    }

    [Test]
    public void ScopedAttribute_AssignsServicesAndEnvironment()
    {
        var attr = new ScopedServiceAttribute(
            "Dev,Prod",
            typeof(IAnimal),
            typeof(IFly)
        );

        Assert.AreEqual(DiScope.Scoped, GetScope(attr));
        Assert.That(
            attr.ServiceTypes.Select(t => t.Name),
            Is.EquivalentTo(
                new[]
                {
                    nameof(IAnimal),
                    nameof(IFly),
                }
            )
        );

        Assert.AreEqual("Dev,Prod", attr.ActiveEnvs);
    }

    [Test]
    public void SingletonAttribute_ActiveEnvOnlyConstructor()
    {
        var attr = new SingletonServiceAttribute("Stage");

        Assert.AreEqual(DiScope.Singleton, GetScope(attr));
        Assert.AreEqual("Stage", attr.ActiveEnvs);
        Assert.IsNull(attr.ServiceTypes);
    }

    [Test]
    public void AttributeHelper_GetAttribute_ReturnsExpectedAttribute()
    {
        var attr = SampleEnum.First.GetAttribute<SampleEnum, SampleAttribute>();

        Assert.AreEqual("FirstValue", attr.Name);
    }

    [Test]
    public void AttributeHelper_GetPropertyInfoByAttr_GenericFiltersProperties()
    {
        var target = new SamplePropertyOwner();

        var props = target.GetPropertyInfoByAttr<SampleAttribute>().ToList();

        Assert.AreEqual(1, props.Count);
        Assert.AreEqual(nameof(SamplePropertyOwner.Decorated), props.Single().Name);
    }

    [Test]
    public void AttributeHelper_GetPropertyInfoByAttr_TypeOverloadMatches()
    {
        var target = new SamplePropertyOwner();

        var props = target.GetPropertyInfoByAttr(typeof(SampleAttribute)).ToList();

        Assert.AreEqual(1, props.Count);
        Assert.AreEqual(nameof(SamplePropertyOwner.Decorated), props.Single().Name);
    }

    [Test]
    public void AssemblyHelper_GetAllTypesOfInterface_FromCurrentAssembly()
    {
        var types = AssemblyHelper.GetAllTypesOfInterface<IAnimal>(typeof(MySingletonService).Assembly).ToList();

        Assert.That(types, Does.Contain(typeof(MySingletonService)));
    }

    [Test]
    public void AssemblyHelper_GetAllTypesByAttribute_WithPrefix()
    {
        var types = AssemblyHelper.GetAllTypesByAttribute<ServiceAttribute>("Yozian.DependencyInjectionPlusTest").ToList();

        Assert.That(types, Does.Contain(typeof(MySingletonService)));
    }

    [Test]
    public void AssemblyHelper_GetAllTypesByAttribute_WithAssembly()
    {
        var types = AssemblyHelper.GetAllTypesByAttribute<ServiceAttribute>(typeof(MySingletonService).Assembly).ToList();

        Assert.That(types, Does.Contain(typeof(MySingletonService)));
    }

    [Test]
    public void AssemblyHelper_GetAllTypesByAttribute_WithUnknownPrefix_ReturnsEmpty()
    {
        var types = AssemblyHelper.GetAllTypesByAttribute<ServiceAttribute>("Unknown.Assembly").ToList();

        Assert.IsEmpty(types);
    }

    private static DiScope GetScope(ServiceAttribute attribute)
    {
        var prop = typeof(ServiceAttribute)
            .GetProperty("DiScope", BindingFlags.Instance | BindingFlags.NonPublic);

        return (DiScope)prop!.GetValue(attribute);
    }

    private enum SampleEnum
    {
        [Sample("FirstValue")]
        First,
    }

    [AttributeUsage(AttributeTargets.All)]
    private sealed class SampleAttribute : Attribute
    {
        public SampleAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }

    private class SamplePropertyOwner
    {
        [Sample("Decorated")]
        public string Decorated { get; set; }

        public string Plain { get; set; }
    }
}
