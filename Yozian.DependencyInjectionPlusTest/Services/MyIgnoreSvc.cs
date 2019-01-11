using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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