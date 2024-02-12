using System;

namespace Jobbr.Server.ForkedExecution.BackChannel.Model
{
    /// <summary>
    /// The job run configuration.
    /// </summary>
    public class JobRunInfoDto
    {
        /// <summary>
        /// The ID of the job.
        /// </summary>
        public long JobId { get; set; }

        /// <summary>
        /// The ID of the job run trigger.
        /// </summary>
        public long TriggerId { get; set; }

        /// <summary>
        /// The user ID.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The user display name.
        /// </summary>
        public string UserDisplayName { get; set; }

        /// <summary>
        /// The ID of the job run.
        /// </summary>
        public long JobRunId { get; set; }

        /// <summary>
        /// The unique <see cref="Guid"/> for the job run.
        /// </summary>
        public Guid UniqueId { get; set; }

        /// <summary>
        /// The parameter for the job.
        /// </summary>
        public object JobParameter { get; set; }

        /// <summary>
        /// The parameter for the instance.
        /// </summary>
        public object InstanceParameter { get; set; }

        /// <summary>
        /// The name of the job.
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// The type of the job.
        /// </summary>
        public string JobType { get; set; }

        /// <summary>
        /// The working directory of the job run.
        /// </summary>
        public string WorkingDir { get; set; }

        /// <summary>
        /// The temporary directory of the job run.
        /// </summary>
        public string TempDir { get; set; }
    }
}
