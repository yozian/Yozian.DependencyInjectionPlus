namespace Yozian.DependencyInjectionPlusTest.Services;

public class CustomTransientService : MyTransientService
{
    public string Marker { get; } = "Customized";
}
