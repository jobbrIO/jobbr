using System;

namespace Jobbr.ComponentModel.Management
{
    public interface IServerManagementService
    {
        int MaxConcurrentJobs { get; set; }

        DateTime StartTime { get; }

        void Shutdown();
    }
}
