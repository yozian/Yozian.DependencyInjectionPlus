using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Yozian.DependencyInjectionPlus.Attributes;

namespace Yozian.DependencyInjectionPlusTest.Services
{
    [TransientService(
        typeof(IAnimal),
        typeof(IFly)
    )]
    public class HashNonImplementInterfaceSvc : IAnimal
    {
        public DateTime CreateAt { get; set; }

        public HashNonImplementInterfaceSvc()
        {
            this.CreateAt = DateTime.Now;
        }

        public void Eat()
        {
        }
    }
}