# Make Dependency Injection Easily

# Features

* Globally auto register services
* Decorate service with attribute
* Auto register service with implement Type also with provided interfaces
* Register services for specify assembly
* Register services for specify assembly prefix name
* Service's type register filter

# Example

## Register for once

```csharp

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

```

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




