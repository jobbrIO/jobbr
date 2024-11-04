using Jobbr.ComponentModel.Execution.Model;

namespace Jobbr.Server.ForkedExecution.BackChannel.Model
{
    /// <summary>
    /// DTO that holds the <see cref="JobRunStates"/>.
    /// </summary>
    public class JobRunUpdateDto
    {
        /// <summary>
        /// States for the job run.
        /// </summary>
        public JobRunStates State { get; set; }
    }
}