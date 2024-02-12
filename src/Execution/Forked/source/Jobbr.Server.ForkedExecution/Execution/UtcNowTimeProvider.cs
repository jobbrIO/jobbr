using System;

namespace Jobbr.Server.ForkedExecution.Execution
{
    /// <summary>
    /// Internal provider for UTC time.
    /// </summary>
    internal class UtcNowTimeProvider : IDateTimeProvider
    {
        /// <inheritdoc/>
        DateTime IDateTimeProvider.GetUtcNow() => DateTime.UtcNow;
    }
}