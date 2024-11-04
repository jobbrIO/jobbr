using System;
using Jobbr.ComponentModel.Execution;
using Jobbr.ComponentModel.Execution.Model;
using Jobbr.Server.ForkedExecution.Execution;

namespace Jobbr.Server.ForkedExecution.Tests.Infrastructure
{
    public class MockedJobContext : IJobRunContext
    {
        private readonly JobRunInfo _jobRunInfo;
        private readonly IJobRunProgressChannel _progressChannel;
        private bool _didReportProgress;

        public MockedJobContext(JobRunInfo jobRunInfo, IJobRunProgressChannel progressChannel)
        {
            _jobRunInfo = jobRunInfo;
            _progressChannel = progressChannel;
        }

        public event EventHandler<JobRunEndedEventArgs> Ended;

        public long JobRunId => _jobRunInfo.Id;

        public void Start()
        {
            _progressChannel.PublishStatusUpdate(_jobRunInfo.Id, JobRunStates.Starting);
        }

        public void RaiseProgressUpdate(double progress)
        {
            _didReportProgress = true;
            _progressChannel.PublishProgressUpdate(_jobRunInfo.Id, progress);
        }

        public void RaiseStatusChange(JobRunStates state)
        {
            _progressChannel.PublishStatusUpdate(_jobRunInfo.Id, state);
        }

        public void RaiseEnded(int exitCode = 0)
        {
            OnEnded(new JobRunEndedEventArgs
            {
                ExitCode = exitCode,
                JobRun = _jobRunInfo,
                DidReportProgress = _didReportProgress,
            });
        }

        protected virtual void OnEnded(JobRunEndedEventArgs e)
        {
            Ended?.Invoke(this, e);
        }
    }
}