using System;

namespace Jobbr.Runtime
{
    public interface IConfigurableServiceProvider : IServiceProvider
    {
        void RegisterInstance<T>(T instance);
    }
}