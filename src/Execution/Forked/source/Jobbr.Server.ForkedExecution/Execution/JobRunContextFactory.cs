using Jobbr.ComponentModel.Execution;
using Jobbr.ComponentModel.Execution.Model;
using Microsoft.Extensions.Logging;

namespace Jobbr.Server.ForkedExecution.Execution
{
    /// <summary>
    /// Factory that creates <see cref="JobRunContext"/>s.
    /// </summary>
    public class JobRunContextFactory : IJobRunContextFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ForkedExecutionConfiguration _configuration;
        private readonly IJobRunProgressChannel _progressChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobRunContextFactory"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">Configuration for the forked execution.</param>
        /// <param name="progressChannel">Job run progress channel.</param>
        public JobRunContextFactory(ILoggerFactory loggerFactory, ForkedExecutionConfiguration configuration, IJobRunProgressChannel progressChannel)
        {
            _loggerFactory = loggerFactory;
            _configuration = configuration;
            _progressChannel = progressChannel;
        }

        /// <inheritdoc/>
        public IJobRunContext CreateJobRunContext(JobRunInfo jobRunInfo)
        {
            return new JobRunContext(_loggerFactory, jobRunInfo, _configuration, _progressChannel);
        }
    }
}