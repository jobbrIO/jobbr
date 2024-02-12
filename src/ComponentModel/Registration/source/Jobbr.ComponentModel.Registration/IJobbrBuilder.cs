using System;

namespace Jobbr.ComponentModel.Registration
{
    public interface IJobbrBuilder
    {
        void Register<T>(Type type);

        void RegisterForCollection<T>(Type type);

        void Add<T>(object instance);

        void AddForCollection<T>(object instance);
    }
}