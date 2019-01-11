using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yozian.DependencyInjectionPlus.Attributes;
namespace DependencyInjectionSample.Services.Impl
{
    [ScopedService(typeof(IWorker), typeof(IDriver))]
    public class WorkService : IWorker, IDriver
    {
        public void DoWork()
        {
            Console.WriteLine(nameof(this.DoWork));
        }

        public void Drive() => throw new NotImplementedException();
    }
}
