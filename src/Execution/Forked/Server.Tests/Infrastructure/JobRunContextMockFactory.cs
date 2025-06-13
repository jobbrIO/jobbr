using System.Collections.Generic;
using System.Linq;
using Jobbr.ComponentModel.Execution;
using Jobbr.ComponentModel.Execution.Model;
using Jobbr.Server.ForkedExecution.Execution;

namespace Jobbr.Server.ForkedExecution.Tests.Infrastructure
{
    internal class JobRunContextMockFactory : IJobRunContextFactory
    {
        private readonly IJobRunProgressChannel _progressChannel;
        private readonly List<MockedJobContext> _contexts = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="JobRunContextMockFactory"/> class.
        /// </summary>
        /// <param name="progressChannel">The job run progress channel.</param>
        public JobRunContextMockFactory(IJobRunProgressChannel progressChannel)
        {
            _progressChannel = progressChannel;
        }

        public long Count => _contexts.Count;

        public MockedJobContext this[long jobRunId] => _contexts.Single(c => c.JobRunId == jobRunId);

        /// <inheritdoc/>
        public IJobRunContext CreateJobRunContext(JobRunInfo jobRunInfo)
        {
            var mockedJobContext = new MockedJobContext(jobRunInfo, _progressChannel);

            _contexts.Add(mockedJobContext);

            return mockedJobContext;
        }
    }
}