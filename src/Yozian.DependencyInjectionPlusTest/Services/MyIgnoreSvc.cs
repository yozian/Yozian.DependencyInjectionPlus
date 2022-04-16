using Yozian.DependencyInjectionPlus.Attributes;

namespace Yozian.DependencyInjectionPlusTest.Services
{
    [TransientService(
        typeof(IAnimal)
    )]
    public class MyIgnoreSvc : ServiceBase, IAnimal
    {
        public void Eat()
        {
        }
    }
}
