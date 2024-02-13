using System;

namespace Jobbr.Server.ForkedExecution.Execution
{
    /// <summary>
    /// Interface for job run contexts.
    /// </summary>
    public interface IJobRunContext
    {
        /// <summary>
        /// Job run end event handler.
        /// </summary>
        event EventHandler<JobRunEndedEventArgs> Ended;

        /// <summary>
        /// Job run ID.
        /// </summary>
        long JobRunId { get; }

        /// <summary>
        /// Start the job run.
        /// </summary>
        void Start();
    }
}