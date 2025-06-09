using System.IO;
using System.Runtime.InteropServices;
using Jobbr.Server.ForkedExecution.Execution;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jobbr.Server.ForkedExecution.Tests.Infrastructure
{
    public class TestBase
    {
        public TestBase()
        {
            JobRunFakeTuples = new FakeGeneratedJobRunsStore();
            ChannelStore = new ProgressChannelStore();
            JobRunInformationService = new JobRunInfoServiceMock(JobRunFakeTuples);
        }

        internal JobRunContextMockFactory RunContextMockFactory { get; private set; }

        protected ProgressChannelStore ChannelStore { get; }

        protected FakeGeneratedJobRunsStore JobRunFakeTuples { get; }
        protected JobRunInfoServiceMock JobRunInformationService { get; }
        protected PeriodicTimerMock TimerMock { get; private set; }
        protected ManualTimeProvider TimeProvider { get; private set; }

        protected static string GetPlatformIndependentExecutableName(string executableName)
        {
            return executableName + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty);
        }

        protected static ForkedExecutionConfiguration GivenAMinimalConfiguration()
        {
            var forkedExecutionConfiguration = new ForkedExecutionConfiguration
            {
                BackendAddress = "notNeeded",
                JobRunDirectory = Path.GetTempPath(),
                JobRunnerExecutable = GetPlatformIndependentExecutableName("Jobbr.Server.ForkedExecution.TestEcho"),
                MaxConcurrentProcesses = 4
            };

            return forkedExecutionConfiguration;
        }

        protected ForkedJobExecutor GivenAMockedExecutor(ForkedExecutionConfiguration forkedExecutionConfiguration)
        {
            RunContextMockFactory = new JobRunContextMockFactory(ChannelStore);

            TimerMock = new PeriodicTimerMock();
            TimeProvider = new ManualTimeProvider();

            var executor = new ForkedJobExecutor(NullLoggerFactory.Instance, RunContextMockFactory, JobRunInformationService, ChannelStore, TimerMock, TimeProvider, forkedExecutionConfiguration);

            return executor;
        }

        protected ForkedJobExecutor GivenAStartedExecutor(ForkedExecutionConfiguration forkedExecutionConfiguration)
        {
            TimerMock = new PeriodicTimerMock();
            TimeProvider = new ManualTimeProvider();

            var jobRunContextFactory = new JobRunContextFactory(NullLoggerFactory.Instance, forkedExecutionConfiguration, ChannelStore);

            var executor = new ForkedJobExecutor(NullLoggerFactory.Instance, jobRunContextFactory, JobRunInformationService, ChannelStore, TimerMock, TimeProvider, forkedExecutionConfiguration);

            executor.Start();

            return executor;
        }
    }
}