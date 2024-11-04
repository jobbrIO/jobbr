using System;

namespace Jobbr.Runtime.ForkedExecution.RestClient
{
    /// <summary>
    /// The job run configuration.
    /// </summary>
    public class JobRunInfoDto
    {
        /// <summary>
        /// Job ID.
        /// </summary>
        public long JobId { get; set; }

        /// <summary>
        /// Trigger ID.
        /// </summary>
        public long TriggerId { get; set; }

        /// <summary>
        /// User ID.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// User display name.
        /// </summary>
        public string UserDisplayName { get; set; }

        /// <summary>
        /// Job run ID.
        /// </summary>
        public long JobRunId { get; set; }

        /// <summary>
        /// Unique ID.
        /// </summary>
        public Guid UniqueId { get; set; }

        /// <summary>
        /// Job parameter.
        /// </summary>
        public object JobParameter { get; set; }

        /// <summary>
        /// Instance parameter.
        /// </summary>
        public object InstanceParameter { get; set; }

        /// <summary>
        /// Job name.
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// Job type.
        /// </summary>
        public string JobType { get; set; }

        /// <summary>
        /// Working directory.
        /// </summary>
        public string WorkingDir { get; set; }

        /// <summary>
        /// Temporary directory.
        /// </summary>
        public string TempDir { get; set; }
    }
}
