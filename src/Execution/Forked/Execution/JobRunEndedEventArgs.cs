using System;
using System.Diagnostics;
using Jobbr.ComponentModel.Execution.Model;

namespace Jobbr.Server.ForkedExecution.Execution
{
    /// <summary>
    /// Event arguments for the job run ended event.
    /// </summary>
    public class JobRunEndedEventArgs : EventArgs
    {
        /// <summary>
        /// Exit code.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// If job run reported progress.
        /// </summary>
        public bool DidReportProgress { get; set; }

        /// <summary>
        /// Job run information.
        /// </summary>
        public JobRunInfo JobRun { get; set; }

        /// <summary>
        /// Process information.
        /// </summary>
        public Process ProcInfo { get; set; }
    }
}