using Yozian.DependencyInjectionPlus.Attributes;

namespace Yozian.DependencyInjectionPlusTest.Services
{
    [SingletonService(
        typeof(IAnimal),
        typeof(IFly)
    )]
    public class MySingletonService : ServiceBase, IAnimal, IFly
    {
        public void Eat()
        {
        }

        public void Fly()
        {
        }
    }
}
