using System;

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
