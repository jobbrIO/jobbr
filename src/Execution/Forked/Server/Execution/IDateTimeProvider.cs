using System;

namespace Jobbr.Server.ForkedExecution.Execution
{
    /// <summary>
    /// Interface for <see cref="DateTime"/> methods.
    /// </summary>
    public interface IDateTimeProvider
    {
        /// <summary>
        /// Gets the current UTC time.
        /// </summary>
        /// <returns>UTC <see cref="DateTime"/>.</returns>
        DateTime GetUtcNow();
    }
}