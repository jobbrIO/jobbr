using System;
using System.Collections.Generic;
using Jobbr.ComponentModel.Registration;

namespace Jobbr.Server.ForkedExecution
{
    /// <summary>
    /// Configuration for forked execution.
    /// </summary>
    public class ForkedExecutionConfiguration : IFeatureConfiguration
    {
        /// <summary>
        /// Address to the backend.
        /// </summary>
        public string BackendAddress { get; set; }

        /// <summary>
        /// Maximum amount of concurrent processes.
        /// </summary>
        public int MaxConcurrentProcesses { get; set; } = 4;

        /// <summary>
        /// The path for the job runner executable.
        /// </summary>
        public string JobRunnerExecutable { get; set; }

        /// <summary>
        /// The path for the job run directory.
        /// </summary>
        public string JobRunDirectory { get; set; }

        /// <summary>
        /// If runtime is waiting for a debugger.
        /// </summary>
        public bool IsRuntimeWaitingForDebugger { get; set; }

        /// <summary>
        /// Callback to add custom parameters to jobrunner.
        /// </summary>
        public Func<JobRunStartInfo, IEnumerable<KeyValuePair<string, string>>> AddJobRunnerArguments { get; set; }
    }
}
