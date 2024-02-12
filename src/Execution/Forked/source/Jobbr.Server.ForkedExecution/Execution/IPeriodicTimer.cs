using System;

namespace Jobbr.Server.ForkedExecution.Execution
{
    /// <summary>
    /// Interface for periodic timers.
    /// </summary>
    public interface IPeriodicTimer
    {
        /// <summary>
        /// Setup the timer.
        /// </summary>
        /// <param name="value">The action that will be performed in intervals.</param>
        /// <param name="intervalInSeconds">The length of the interval in seconds.</param>
        void Setup(Action value, long intervalInSeconds);

        /// <summary>
        /// Start the timer.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the timer.
        /// </summary>
        void Stop();
    }
}