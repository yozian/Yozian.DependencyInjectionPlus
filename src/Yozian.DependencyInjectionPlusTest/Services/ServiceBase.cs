using System;
using System.Collections.Generic;
using System.Text;

namespace Yozian.DependencyInjectionPlusTest.Services
{
    public class ServiceBase
    {
        public string Name { get; set; } = "default";

        public DateTime CreateAt { get; set; }

        public ServiceBase()
        {
            this.CreateAt = DateTime.Now;
        }

        public void DoWork()
        {
            Console.WriteLine($"{this.GetType().Name}: work {DateTime.Now.ToString("HHmmssfff")}");
        }
    }
}