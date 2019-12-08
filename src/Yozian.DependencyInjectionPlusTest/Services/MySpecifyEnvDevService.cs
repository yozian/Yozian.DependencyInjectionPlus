using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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