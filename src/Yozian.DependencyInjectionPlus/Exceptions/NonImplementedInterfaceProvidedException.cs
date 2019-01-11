using System;
using System.Collections.Generic;
using System.Text;

namespace Yozian.DependencyInjectionPlus.Exceptions
{
    public class NonImplementedInterfaceProvidedException : Exception
    {
        public NonImplementedInterfaceProvidedException(string message)
            : base(message)
        {
        }
    }
}