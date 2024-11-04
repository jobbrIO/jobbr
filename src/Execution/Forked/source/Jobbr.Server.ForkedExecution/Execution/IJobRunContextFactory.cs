using Jobbr.ComponentModel.Execution.Model;

namespace Jobbr.Server.ForkedExecution.Execution
{
    /// <summary>
    /// Interface for job run context factories.
    /// </summary>
    public interface IJobRunContextFactory
    {
        /// <summary>
        /// Creates a new <see cref="IJobRunContext"/>.
        /// </summary>
        /// <param name="jobRunInfo">Job run info for the <see cref="IJobRunContext"/>.</param>
        /// <returns>A new <see cref="IJobRunContext"/>.</returns>
        IJobRunContext CreateJobRunContext(JobRunInfo jobRunInfo);
    }
}