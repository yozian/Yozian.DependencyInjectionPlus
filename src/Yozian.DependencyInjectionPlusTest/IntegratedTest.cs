using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Yozian.DependencyInjectionPlus;
using Yozian.DependencyInjectionPlus.Exceptions;
using Yozian.DependencyInjectionPlusTest.Services;

namespace Yozian.DependencyInjectionPlusTest
{
    public class IntegratedTest
    {
        private ServiceProvider provider;

        private ILogger logger;

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

            var container = new ServiceCollection();
            var loggerFactory = LoggerFactory.Create(options => { options.AddConsole(); });

            this.logger = loggerFactory.CreateLogger<IntegratedTest>();

            container.RegisterServices(
                "Yozian.DependencyInjectionPlusTest", // leave empty for all assemblies loaded!
                t => // leave empty for all types with specify attribute to be registered
                {
                    // filter the type you want
                    return t.Name.Contains("Service");
                },
                this.logger
            );

            this.provider = container.BuildServiceProvider();

            var mySingleton = this.provider.GetService<MySingletonService>();

            mySingleton.Name = "Setup";
        }

        [Test]
        public void ConcreteTypeDiTest()
        {
            var service = this.provider.GetService<MyTransientService>();

            var service2 = this.provider.GetService<MyTransientService>();

            Assert.AreNotEqual(service.GetHashCode(), service2.GetHashCode());
        }

        [Test]
        public void InterfaceTypeDiTest()
        {
            var services = this.provider.GetServices<IFly>();

            services.First().Fly();

            Assert.Pass();
        }

        [Test]
        public void ScopedIdentifyTest()
        {
            var scope = this.provider.CreateScope();

            // test the same scope
            var service = scope.ServiceProvider.GetService<MyScopedService>();

            service.Name = "abc";

            var service2 = scope.ServiceProvider.GetService<MyScopedService>();

            Assert.AreEqual(service.Name, service2.Name);
            Assert.AreEqual(service.CreateAt, service2.CreateAt);

            // test with default provider
            var service3 = this.provider.GetService<MyScopedService>();

            Assert.AreNotEqual(service.Name, service3.Name);

            // test with new scope
            var scope2 = this.provider.CreateScope();

            var service4 = this.provider.GetService<MyScopedService>();

            Assert.AreNotEqual(service.Name, service4.Name);
        }

        [Test]
        public void SingletonIdentifyTest()
        {
            var service = this.provider.GetService<MySingletonService>();

            Assert.AreEqual("Setup", service.Name);
        }

        [Test]
        public void ShouldFilterOutTest()
        {
            var service = this.provider.GetService<MyIgnoreSvc>();

            Assert.AreEqual(null, service);
        }

        [Test]
        public void InjectFilterTest()
        {
            var container = new ServiceCollection();
            container.RegisterServices(
                "Yozian.DependencyInjectionPlusTest",
                t => t.Name.Contains("MyIgnoreSvc")
            );

            var p = container.BuildServiceProvider();

            var service = p.GetService<MyIgnoreSvc>();

            Assert.AreEqual(typeof(MyIgnoreSvc), service.GetType());
        }

        [Test]
        public void HashNonImplementInterfaceProvidedTest()
        {
            Assert.Throws(
                typeof(NonImplementedInterfaceProvidedException),
                () =>
                {
                    var container = new ServiceCollection();
                    container.RegisterServices(
                        "Yozian.DependencyInjectionPlusTest",
                        t => t.Name.Contains("HashNonImplementInterfaceSvc")
                    );
                }
            );
        }

        [Test]
        public void SpecifyEnvActiveDevTest()
        {
            var svc = this.provider.GetService<MySpecifyEnvDevService>();

            svc.DoWork();
        }

        [Test]
        public void SpecifyEnvNonActiveTest()
        {
            Assert.Throws<NullReferenceException>(
                () =>
                {
                    var svc = this.provider.GetService<MySpecifyEnvStagingService>();

                    svc.DoWork();
                }
            );
        }
    }
}
