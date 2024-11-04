namespace Jobbr.Runtime.ForkedExecution
{
    /// <summary>
    /// Job run states.
    /// </summary>
    public enum JobRunStates
    {
        Null,
        Scheduled,
        Preparing,
        Starting,
        Started,
        Connected,
        Initializing,
        Processing,
        Finishing,
        Collecting,
        Completed,
        Failed,
        Deleted,
        Omitted
    }
}