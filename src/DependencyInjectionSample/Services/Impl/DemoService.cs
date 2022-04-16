using System;
using Yozian.DependencyInjectionPlus.Attributes;

namespace DependencyInjectionSample.Services.Impl
{
    [TransientService]
    public class DemoService
    {
        public void ShowTime()
        {
            Console.WriteLine(DateTime.Now.ToString());
        }
    }
}
