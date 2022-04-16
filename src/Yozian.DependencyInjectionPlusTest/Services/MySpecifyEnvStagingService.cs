using Yozian.DependencyInjectionPlus.Attributes;

namespace Yozian.DependencyInjectionPlusTest.Services
{
    [TransientService("Staging")]
    public class MySpecifyEnvStagingService : ServiceBase, IAnimal, IFly
    {
        public void Eat()
        {
        }

        public void Fly()
        {
        }
    }
}
