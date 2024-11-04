using System;
using System.Collections.Generic;

namespace Jobbr.ComponentModel.Registration
{
    public interface IJobbrServiceProvider
    {
        object GetService(Type serviceType);

        IEnumerable<object> GetServices(Type serviceType);

        IJobbrServiceProvider GetChild();
    }
}
