using System;

namespace Yozian.DependencyInjectionPlusTest.Services
{
    public class ServiceBase: IName
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
