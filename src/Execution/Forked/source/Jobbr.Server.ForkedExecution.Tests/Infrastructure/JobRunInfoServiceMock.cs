using Jobbr.ComponentModel.Execution;
using Jobbr.ComponentModel.Execution.Model;

namespace Jobbr.Server.ForkedExecution.Tests.Infrastructure
{
    /// <summary>
    /// Wraps the in-memory <see cref="FakeGeneratedJobRunsStore"/> to be able to work as a <see cref="IJobRunInformationService"/>.
    /// </summary>
    public class JobRunInfoServiceMock : IJobRunInformationService
    {
        private readonly FakeGeneratedJobRunsStore _fakeStore;

        public JobRunInfoServiceMock(FakeGeneratedJobRunsStore fakeStore)
        {
            _fakeStore = fakeStore;
        }

        public JobRunInfo GetByJobRunId(long jobRunId)
        {
            return _fakeStore.GetByJobRunId(jobRunId)?.JobRunInfo;
        }
    }
}