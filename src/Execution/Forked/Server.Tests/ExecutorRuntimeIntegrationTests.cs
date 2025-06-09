using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Jobbr.ComponentModel.Execution;
using Jobbr.ComponentModel.Execution.Model;
using Jobbr.Server.ForkedExecution.BackChannel;
using Jobbr.Server.ForkedExecution.TestRunner.TestJobs;
using Jobbr.Server.ForkedExecution.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector;

namespace Jobbr.Server.ForkedExecution.Tests
{
    [TestClass]
    public class ExecutorRuntimeIntegrationTests : TestBase
    {
        [TestMethod]
        public void Startup_ExecutorAndBackChannel_BothRunning()
        {
            // Arrange
            var config = GivenAMinimalConfiguration();
            GivenAStartedBackChannelHost(config);
            GivenAStartedExecutor(config);

            // Act
            var statusResponse = new HttpClient().GetAsync(config.BackendAddress + "/fex/jobrun/524868244").Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, statusResponse.StatusCode);
        }

        [TestMethod]
        public void PlannedJob_AfterStart_WillCallback()
        {
            // Arrange
            var config = GivenAMinimalConfiguration();
            config.JobRunnerExecutable = GetPlatformIndependentExecutableName("Jobbr.Server.ForkedExecution.TestRunner");

            GivenAStartedBackChannelHost(config);
            var executor = GivenAStartedExecutor(config);

            var fakeRun = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);

            // Act
            TimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            // Assert
            var hasConnected = ChannelStore.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.Id].Contains(JobRunStates.Connected), 3000);

            Assert.IsTrue(hasConnected, "The runner executable should connect within 1s");

            // Teardown: Wait for Failing or Completed state to that the executable is able to exit
            ChannelStore.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.Id].Contains(JobRunStates.Failed) || allUpdates[fakeRun.Id].Contains(JobRunStates.Completed), 10000);
        }

        [TestMethod]
        public void PlannedJob_AfterStart_PidIsSet()
        {
            // Arrange
            var config = GivenAMinimalConfiguration();
            config.JobRunnerExecutable = GetPlatformIndependentExecutableName("Jobbr.Server.ForkedExecution.TestRunner");

            GivenAStartedBackChannelHost(config);
            var executor = GivenAStartedExecutor(config);

            var fakeRun = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);

            // Act
            TimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            var hasConnected = ChannelStore.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.Id].Contains(JobRunStates.Connected), 3000);

            // Assert
            Assert.IsTrue(hasConnected, "The runner executable should connect within 1s");

            var hasPid = ChannelStore.AllPids.ContainsKey(fakeRun.Id);

            Assert.IsTrue(hasPid, "There should be a Pid stored for this jobRun");
            Assert.IsTrue(ChannelStore.AllPids[fakeRun.Id].First().Item2 > 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(ChannelStore.AllPids[fakeRun.Id].First().Item1));
        }

        [TestMethod]
        public void PlannedJob_AfterStarting_FoldersExist()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetTempFileName().Replace(".", string.Empty));

            var config = GivenAMinimalConfiguration();
            config.JobRunnerExecutable = GetPlatformIndependentExecutableName("Jobbr.Server.ForkedExecution.TestEcho");
            config.JobRunDirectory = tempDir;

            var executor = GivenAStartedExecutor(config);

            // Act
            var fakeRun = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);

            TimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            // Assert
            var hasStarted = ChannelStore.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.Id].Contains(JobRunStates.Starting), 3000);
            Assert.IsTrue(hasStarted, "The runner executable should have started, but did not within 3s");

            Assert.IsTrue(Directory.Exists(tempDir), "The temp directory should haven been created");
            Assert.IsTrue(Directory.Exists(Path.Combine(tempDir, $"jobbr-{fakeRun.Id}")), "The run directory should exist");
            Assert.IsTrue(Directory.Exists(Path.Combine(tempDir, $"jobbr-{fakeRun.Id}", "work")), "The run\\work directory should exist");
        }

        [TestMethod]
        public void RunnerExecutable_WithInvalidJob_ReturnsFailed()
        {
            // Arrange
            var config = GivenAMinimalConfiguration();
            config.JobRunnerExecutable = GetPlatformIndependentExecutableName("Jobbr.Server.ForkedExecution.TestRunner");

            GivenAStartedBackChannelHost(config);
            var executor = GivenAStartedExecutor(config);

            var fakeRun = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            fakeRun.JobRunInfo.Type = "NotExistentClrType";

            // Act
            TimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            // Assert
            var hasCompleted = ChannelStore.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.Id].Contains(JobRunStates.Failed), 3000);

            Assert.IsTrue(hasCompleted, "The runner executable should fail, but did not within 3s");
            Assert.IsTrue(ChannelStore.AllStatusUpdates[fakeRun.Id].Contains(JobRunStates.Failed), "There should be a failed state for this jobRun");
        }

        [TestMethod]
        public void RunnerExecutable_GetsValidJob_ReturnsCompleted()
        {
            // Arrange
            var config = GivenAMinimalConfiguration();
            config.JobRunnerExecutable = GetPlatformIndependentExecutableName("Jobbr.Server.ForkedExecution.TestRunner");

            GivenAStartedBackChannelHost(config);
            var executor = GivenAStartedExecutor(config);

            var fakeRun = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            fakeRun.JobRunInfo.Type = "SimpleJob";

            // Act
            TimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            // Assert
            var hasCompleted = ChannelStore.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.Id].Contains(JobRunStates.Completed), 3000);

            Assert.IsTrue(hasCompleted, "The runner executable should completed, but did not within 3s");
            Assert.IsTrue(ChannelStore.AllStatusUpdates[fakeRun.Id].Contains(JobRunStates.Completed), "There should be a completed state for this jobRun");
        }

        [TestMethod]
        public void RunnerExecutable_ParamsJobWithoutParams_Completes()
        {
            // Arrange
            var config = GivenAMinimalConfiguration();
            config.JobRunnerExecutable = GetPlatformIndependentExecutableName("Jobbr.Server.ForkedExecution.TestRunner");

            GivenAStartedBackChannelHost(config);
            var executor = GivenAStartedExecutor(config);

            var fakeRun = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            fakeRun.JobRunInfo.Type = "JobWithParameters";

            // Act
            TimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            // Assert
            var hasCompleted = ChannelStore.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.Id].Contains(JobRunStates.Completed), 3000);

            Assert.IsTrue(hasCompleted, "The runner executable should complete, but did not within 3s");
            Assert.IsTrue(ChannelStore.AllStatusUpdates[fakeRun.Id].Contains(JobRunStates.Completed), "There should be a completed state for this jobRun");
        }

        [TestMethod]
        public void RunnerExecutable_JobWithArtefacts_UploadsFiles()
        {
            // Arrange
            var config = GivenAMinimalConfiguration();
            config.JobRunnerExecutable = GetPlatformIndependentExecutableName("Jobbr.Server.ForkedExecution.TestRunner");

            GivenAStartedBackChannelHost(config);
            var executor = GivenAStartedExecutor(config);

            var fakeRun = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            fakeRun.JobRunInfo.Type = "JobWithArtefacts";

            // Act
            TimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            // Assert
            var hasCompleted = ChannelStore.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.Id].Contains(JobRunStates.Completed), 3000);

            Assert.IsTrue(hasCompleted, "The runner executable should completed, but did not within 3s");
            Assert.IsTrue(ChannelStore.AllUploadedArtefacts[fakeRun.Id].Any(), "It should contain at least one file!");
        }

        [TestMethod]
        public void RunnerExecutable_JobWithProgress_SendsMultipleStates()
        {
            // Arrange
            var config = GivenAMinimalConfiguration();
            config.JobRunnerExecutable = GetPlatformIndependentExecutableName("Jobbr.Server.ForkedExecution.TestRunner");

            GivenAStartedBackChannelHost(config);
            var executor = GivenAStartedExecutor(config);

            var fakeRun = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            fakeRun.JobRunInfo.Type = "SimpleJob";

            // Act
            TimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            ChannelStore.WaitForStatusUpdate(updatesFromAllJobs => updatesFromAllJobs[fakeRun.Id].Contains(JobRunStates.Completed), 3000);

            // Assert
            Assert.IsTrue(ChannelStore.AllStatusUpdates[fakeRun.Id].Contains(JobRunStates.Connected), "It should contain the Connected state");
            Assert.IsTrue(ChannelStore.AllStatusUpdates[fakeRun.Id].Contains(JobRunStates.Initializing), "It should contain the Initializing state");
            Assert.IsTrue(ChannelStore.AllStatusUpdates[fakeRun.Id].Contains(JobRunStates.Processing), "It should contain the Processing state");
            Assert.IsTrue(ChannelStore.AllStatusUpdates[fakeRun.Id].Contains(JobRunStates.Finishing), "It should contain the Finishing state");
            Assert.IsTrue(ChannelStore.AllStatusUpdates[fakeRun.Id].Contains(JobRunStates.Collecting), "It should contain the Collecting state");
            Assert.IsTrue(ChannelStore.AllStatusUpdates[fakeRun.Id].Contains(JobRunStates.Completed), "It should contain the Completed state");
        }

        [TestMethod]
        public void RunnerExecutable_JobWithProgress_MultipleStatus()
        {
            // Arrange
            var config = GivenAMinimalConfiguration();
            config.JobRunnerExecutable = GetPlatformIndependentExecutableName("Jobbr.Server.ForkedExecution.TestRunner");

            GivenAStartedBackChannelHost(config);
            var executor = GivenAStartedExecutor(config);

            var fakeRun = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            fakeRun.JobRunInfo.Type = "JobWithOneProgress";

            // Act
            TimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            ChannelStore.WaitForProgressUpdate(allUpdates => allUpdates[fakeRun.Id].Contains(JobWithOneProgress.DefinedProgressValue), 3000);

            // Assert
            Assert.IsTrue(ChannelStore.AllProgressUpdates[fakeRun.Id].Contains(JobWithOneProgress.DefinedProgressValue), "The runner executable should have send one progress update");
        }

        [TestMethod]
        public void RunnerExecutable_SuccessfulJobWithProgress_SetTo100Percent()
        {
            // Arrange
            var config = GivenAMinimalConfiguration();
            config.JobRunnerExecutable = GetPlatformIndependentExecutableName("Jobbr.Server.ForkedExecution.TestRunner");

            GivenAStartedBackChannelHost(config);
            var executor = GivenAStartedExecutor(config);

            var fakeRun = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            fakeRun.JobRunInfo.Type = "JobWithOneProgress";

            // Act
            TimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            // Assert
            var hasCompleted = ChannelStore.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.Id].Contains(JobRunStates.Completed), 3000);

            Assert.IsTrue(hasCompleted, "The runner executable should completed, but did not within 3s");
            ChannelStore.WaitForProgressUpdate(allUpdates => allUpdates[fakeRun.Id].Count == 2, 3000);

            Assert.AreEqual(2, ChannelStore.AllProgressUpdates[fakeRun.Id].Count, "Got: [" + string.Join(", ", ChannelStore.AllProgressUpdates[fakeRun.Id]) + "]");
            Assert.AreEqual(JobRunStates.Completed, ChannelStore.AllStatusUpdates[fakeRun.Id].Last(), "The job did not complete");
            Assert.AreEqual(100, ChannelStore.AllProgressUpdates[fakeRun.Id][1], "It should have announced 100% completion after successful exit");
        }

        [TestMethod]
        public void RunnerExecutable_FailingJobWithProgress_DontTouchProgress()
        {
            // Arrange
            var config = GivenAMinimalConfiguration();
            config.JobRunnerExecutable = GetPlatformIndependentExecutableName("Jobbr.Server.ForkedExecution.TestRunner");

            GivenAStartedBackChannelHost(config);
            var executor = GivenAStartedExecutor(config);

            var fakeRun = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            fakeRun.JobRunInfo.Type = "JobWithOneProgress";
            fakeRun.JobRunInfo.InstanceParameters = JsonSerializer.Serialize(new { ShouldFail = true }, DefaultJsonOptions.Options);

            // Act
            TimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            // Assert
            var hasCompleted = ChannelStore.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.Id].Contains(JobRunStates.Failed), 3000);

            Assert.IsTrue(hasCompleted, "The runner executable should have failed in the meantime, but did not within 3s");

            ChannelStore.WaitForProgressUpdate(allUpdates => allUpdates[fakeRun.Id].Contains(JobWithOneProgress.DefinedProgressValue), 3000);

            Assert.IsTrue(ChannelStore.AllProgressUpdates.ContainsKey(fakeRun.Id), "There should be any progress update");
            Assert.AreEqual(1, ChannelStore.AllProgressUpdates[fakeRun.Id].Count, "Got: [" + string.Join(", ", ChannelStore.AllProgressUpdates[fakeRun.Id]) + "]");
            Assert.AreEqual(JobRunStates.Failed, ChannelStore.AllStatusUpdates[fakeRun.Id].Last(), "The job did not complete");
        }

        [TestMethod]
        public void RunnerExecutable_AdditionalParameters_PassedToExecutable()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetTempFileName().Replace(".", string.Empty));
            Directory.CreateDirectory(tempDir);

            var config = GivenAMinimalConfiguration();
            config.JobRunnerExecutable = GetPlatformIndependentExecutableName("Jobbr.Server.ForkedExecution.TestEcho");
            config.JobRunDirectory = tempDir;
            config.AddJobRunnerArguments = _ => new List<KeyValuePair<string, string>>(new[] { new KeyValuePair<string, string>("argument1", "value1"), });

            GivenAStartedBackChannelHost(config);
            var executor = GivenAStartedExecutor(config);

            var fakeRun = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            var expectedWorkdir = Path.Combine(tempDir, $"jobbr-{fakeRun.Id}", "work");

            // Act
            TimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            // Wait until the process has exited
            var hasCompleted = ChannelStore.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.Id].Contains(JobRunStates.Completed), 3000);

            // Assert
            Assert.IsTrue(hasCompleted, "The Echo Executable did not complete withing the timeout");
            Assert.IsTrue(Directory.Exists(expectedWorkdir), "The run\\work directory should exist");
            Assert.IsTrue(Directory.EnumerateFiles(expectedWorkdir).Any(), "There should be one file");

            var fileContent = File.ReadAllText(Directory.EnumerateFiles(expectedWorkdir).First());
            Assert.IsNotNull(fileContent);
            Assert.IsTrue(fileContent.Contains("--argument1\nvalue1"), $"Expected to find arguments in content, but got '{fileContent}'");
        }

        [TestMethod]
        public void RunnerExecutable_WhiteSpaceParameters_PassedToExecutable()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetTempFileName().Replace(".", string.Empty));
            Directory.CreateDirectory(tempDir);

            var config = GivenAMinimalConfiguration();
            config.JobRunnerExecutable = GetPlatformIndependentExecutableName("Jobbr.Server.ForkedExecution.TestEcho");
            config.JobRunDirectory = tempDir;
            config.AddJobRunnerArguments = _ => new List<KeyValuePair<string, string>>(new[] { new KeyValuePair<string, string>("arg", "v is with whitespace"), });

            GivenAStartedBackChannelHost(config);
            var executor = GivenAStartedExecutor(config);

            var fakeRun = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            var expectedWorkdir = Path.Combine(tempDir, $"jobbr-{fakeRun.Id}", "work");

            // Act
            TimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            // Wait until process has exited
            var hasCompleted = ChannelStore.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.Id].Contains(JobRunStates.Completed), 3000);

            // Assert
            Assert.IsTrue(hasCompleted, "The Echo Executable did not complete withing the timeout");
            Assert.IsTrue(Directory.Exists(expectedWorkdir), "The run\\work directory should exist");
            Assert.IsTrue(Directory.EnumerateFiles(expectedWorkdir).Any(), "There should be one file");

            var fileContent = File.ReadAllText(Directory.EnumerateFiles(expectedWorkdir).First());
            Assert.IsNotNull(fileContent);
            Assert.IsTrue(fileContent.Contains("--arg\nv is with whitespace"), $"Expected to find arguments in content, but got '{fileContent}'");
        }

        private void GivenAStartedBackChannelHost(ForkedExecutionConfiguration config)
        {
            config.BackendAddress = string.Empty;

            new ConfigurationValidator(NullLoggerFactory.Instance).Validate(config);

            var serviceCollection = new Container();
            serviceCollection.RegisterInstance<IJobRunInformationService>(JobRunInformationService);
            serviceCollection.RegisterInstance<IJobRunProgressChannel>(ChannelStore);

            var backChannelHost = new BackChannelWebHost(NullLoggerFactory.Instance, serviceCollection, config);
            backChannelHost.Start();
        }
    }
}
