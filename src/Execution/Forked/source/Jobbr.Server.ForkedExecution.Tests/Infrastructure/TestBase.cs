using System.IO;
using System.Runtime.InteropServices;
using Jobbr.Server.ForkedExecution.Execution;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jobbr.Server.ForkedExecution.Tests.Infrastructure
{
    public class TestBase
    {
        protected ProgressChannelStore ProgressChannelStore;

        protected FakeGeneratedJobRunsStore JobRunFakeTuples;
        protected JobRunInfoServiceMock JobRunInformationService;
        protected PeriodicTimerMock PeriodicTimerMock;
        protected ManualTimeProvider ManualTimeProvider;
        internal JobRunContextMockFactory JobRunContextMockFactory;

        public TestBase()
        {
            JobRunFakeTuples = new FakeGeneratedJobRunsStore();
            ProgressChannelStore = new ProgressChannelStore();
            JobRunInformationService = new JobRunInfoServiceMock(JobRunFakeTuples);
        }

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
            JobRunContextMockFactory = new JobRunContextMockFactory(ProgressChannelStore);

            PeriodicTimerMock = new PeriodicTimerMock();
            ManualTimeProvider = new ManualTimeProvider();

            var executor = new ForkedJobExecutor(NullLoggerFactory.Instance, JobRunContextMockFactory, JobRunInformationService, ProgressChannelStore, PeriodicTimerMock, ManualTimeProvider, forkedExecutionConfiguration);

            return executor;
        }

        protected ForkedJobExecutor GivenAStartedExecutor(ForkedExecutionConfiguration forkedExecutionConfiguration)
        {
            PeriodicTimerMock = new PeriodicTimerMock();
            ManualTimeProvider = new ManualTimeProvider();

            var jobRunContextFactory = new JobRunContextFactory(NullLoggerFactory.Instance, forkedExecutionConfiguration, ProgressChannelStore);

            var executor = new ForkedJobExecutor(NullLoggerFactory.Instance, jobRunContextFactory, JobRunInformationService, ProgressChannelStore, PeriodicTimerMock, ManualTimeProvider, forkedExecutionConfiguration);

            executor.Start();

            return executor;
        }
    }
}