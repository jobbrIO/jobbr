using System;
using System.Collections.Generic;
using Jobbr.ComponentModel.Execution.Model;

namespace Jobbr.ComponentModel.Execution
{
    /// <summary>
    /// The JobStarter interface.
    /// </summary>
    public interface IJobExecutor : IDisposable
    {
        void Start();

        void OnPlanChanged(List<PlannedJobRun> newPlan);

        bool OnJobRunCanceled(long id);

        void Stop();
    }
}