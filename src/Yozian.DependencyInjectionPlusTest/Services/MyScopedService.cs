using Yozian.DependencyInjectionPlus.Attributes;

namespace Yozian.DependencyInjectionPlusTest.Services
{
    [ScopedService(
        typeof(IAnimal),
        typeof(IFly)
    )]
    public class MyScopedService : ServiceBase, IAnimal, IFly
    {
        public void Eat()
        {
        }

        public void Fly()
        {
        }
    }
}
