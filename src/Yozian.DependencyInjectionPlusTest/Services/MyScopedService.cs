using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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