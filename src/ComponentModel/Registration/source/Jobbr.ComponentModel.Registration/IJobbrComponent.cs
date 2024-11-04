using System;

namespace Jobbr.ComponentModel.Registration
{
    public interface IJobbrComponent : IDisposable
    {
        void Start();

        void Stop();
    }
}