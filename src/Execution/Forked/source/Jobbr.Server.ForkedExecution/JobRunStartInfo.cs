namespace Jobbr.Server.ForkedExecution
{
    /// <summary>
    /// Jobbr run start information.
    /// </summary>
    public class JobRunStartInfo
    {
        /// <summary>
        /// Job ID.
        /// </summary>
        public long JobId { get; internal set; }

        /// <summary>
        /// Unique name.
        /// </summary>
        public string UniqueName { get; internal set; }

        /// <summary>
        /// Job type.
        /// </summary>
        public string JobType { get; internal set; }

        /// <summary>
        /// Job run ID.
        /// </summary>
        public long JobRunId { get; internal set; }

        /// <summary>
        /// Job run trigger ID.
        /// </summary>
        public long TriggerId { get; internal set; }

        /// <summary>
        /// User ID.
        /// </summary>
        public string UserId { get; internal set; }
    }
}