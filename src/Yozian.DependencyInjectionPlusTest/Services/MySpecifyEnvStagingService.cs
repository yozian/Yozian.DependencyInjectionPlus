using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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