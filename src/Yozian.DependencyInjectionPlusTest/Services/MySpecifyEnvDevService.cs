using Yozian.DependencyInjectionPlus.Attributes;

namespace Yozian.DependencyInjectionPlusTest.Services
{
    [TransientService("Dev,Production")]
    public class MySpecifyEnvDevService : ServiceBase, IAnimal, IFly
    {
        public void Eat()
        {
        }

        public void Fly()
        {
        }
    }
}
