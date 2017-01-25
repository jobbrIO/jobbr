namespace Jobbr.Server.ForkedExecution.Core.ServiceMessaging
{
    /// <summary>
    /// The progress service message.
    /// </summary>
    public class ProgressServiceMessage : ServiceMessage
    {
        /// <summary>
        /// Gets or sets the percent.
        /// </summary>
        public double Percent { get; set; }
    }
}