using Yozian.DependencyInjectionPlus.Attributes;

namespace Yozian.DependencyInjectionPlusTest.Services
{
    [TransientService(
        typeof(IAnimal),
        typeof(IFly)
    )]
    public class MyTransientService : ServiceBase, IAnimal, IFly
    {
        public void Eat()
        {
        }

        public void Fly()
        {
        }
    }
}
