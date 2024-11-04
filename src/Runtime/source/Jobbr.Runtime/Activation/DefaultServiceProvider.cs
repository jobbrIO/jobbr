using System;

namespace Jobbr.Runtime.Activation
{
    /// <summary>
    /// The default service provider is able to activate job types without any dependencies
    /// </summary>
    internal class DefaultServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            return Activator.CreateInstance(serviceType);
        }
    }
}