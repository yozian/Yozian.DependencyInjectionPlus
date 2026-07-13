# Make Dependency Injection Easily

# Features

* Write Once, Register all.
* Register services globally which decorated by attributes(TransientService, ScopedService, SingletonService)
* Auto register service with implement Type also with provided interfaces.
* Register services for specify assembly.
* Service's type filter while regstering.
* Register method is an extension method on IServiceColletion
* Register services with specify environments
* Plug a service resolver delegate or an `IServiceResolver` instance to provide custom concrete implementations at registration time

# Example

## Register for once

```csharp
    
    // sample for .net core 
     public void ConfigureServices(IServiceCollection services)
     {
        // just choose one of them to fit your scenario


        // 1. register services
        services.RegisterServices();

        // 2. register services only in those assmbly name start with "DependencyInjectionSample"
        services.RegisterServices("DependencyInjectionSample");


        // 3. register services and has some condition
        services.RegisterServices("", type =>
        {
            return type.Name.Contains("Service");
        });


        // 4. regiter services for specify assembly
        services.RegisterServicesOfAssembly(new { }.GetType().Assembly);

        // 5. override concrete instances with a custom resolver
        services.RegisterServices(
            serviceResolver: (provider, serviceType) =>
            {
                if (serviceType == typeof(MyTransientService))
                {
                    return new MyCustomTransientService();
                }

                return Activator.CreateInstance(serviceType);
            }
        );

        services.RegisterServices(
            serviceResolverInstance: new MyCustomServiceResolver()
        );
    }

```

## Custom Service Resolver

Pass a `Func<IServiceProvider, Type, object>` or an `IServiceResolver` instance into `RegisterServices` (or `RegisterServicesOfAssembly`) to decide how each decorated service is instantiated. This makes it easy to inject decorators, proxies, or test doubles without changing every attribute.

```csharp
services.RegisterServices(
    "MyApp",
    serviceResolver: (provider, serviceType) =>
    {
        if (serviceType == typeof(PaymentGateway))
        {
            return new LoggingPaymentGateway(new PaymentGateway());
        }

        return ActivatorUtilities.CreateInstance(provider, serviceType);
    }
);
```

### IServiceResolver interface

```csharp
public interface IServiceResolver
{
    object Resolve(IServiceProvider provider, Type serviceType);
}
```

Supplying `serviceResolverInstance: new MyResolver()` lets you encapsulate any custom creation logic (or reuse an existing resolver singleton) without modifying every attribute-decorated service.

## Decorated Service

```csharp

    [TransientService]
    public class DemoService
    {

        public void ShowTime()
        {
            Console.WriteLine(DateTime.Now.ToString());
        }
    }

    [ScopedService]
    public class DemoService
    {

        public void ShowTime()
        {
            Console.WriteLine(DateTime.Now.ToString());
        }
    }


    [SingletonService]
    public class DemoService
    {

        public void ShowTime()
        {
            Console.WriteLine(DateTime.Now.ToString());
        }
    }


```

## Decoreated service with interfaces

Provide one or more interface types

```csharp

    [ScopedService(typeof(IWorker), typeof(IDriver))]
    public class WorkService : IWorker, IDriver
    {
        public void DoWork()
        {
            Console.WriteLine(nameof(this.DoWork));
        }

        public void Drive() => throw new NotImplementedException();
    }

```


## Decoreated service with actived Environments

Register only in Developement & Staging environment(by Environment variable "ASPNETCORE_ENVIRONMENT")

```csharp

    [ScopedService("Developement,Staging")]
    public class WorkService : IWorker, IDriver
    {
        public void DoWork()
        {
            Console.WriteLine(nameof(this.DoWork));
        }

        public void Drive() => throw new NotImplementedException();
    }

```


## Output registered types in STD out

DependencyInjectionSample project shows

```
Register Transient Services , Total: 1
     [ConcretType : SerivceTypes]
     (1) DemoService: DemoService
Register Scoped Services , Total: 1
     [ConcretType : SerivceTypes]
     (1) WorkService: WorkService, IWorker
```

## License

MIT. See the `LICENSE` file for details.




