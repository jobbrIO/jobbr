namespace Jobbr.Runtime.ForkedExecution.RestClient
{
    /// <summary>
    /// Job run update data transfer object.
    /// </summary>
    public class JobRunUpdateDto
    {
        /// <summary>
        /// Job run state.
        /// </summary>
        public JobRunStates State { get; set; }
    }
}