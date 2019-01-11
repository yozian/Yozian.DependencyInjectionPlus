using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
