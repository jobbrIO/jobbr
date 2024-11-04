using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Jobbr.ComponentModel.Execution.Model;
using Jobbr.Server.ForkedExecution.BackChannel;
using Jobbr.Server.ForkedExecution.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.Server.ForkedExecution.Tests
{
    [TestClass]
    public class ExecutionTests : TestBase
    {
        private void GivenAStartedBackChannelHost(ForkedExecutionConfiguration config)
        {
            config.BackendAddress = string.Empty;

            var backChannelHost = new BackChannelWebHost(new JobbrServiceProviderMock(this.jobRunInformationService, this.storedProgressUpdates), config);
            backChannelHost.Start();
        }

        [TestMethod]
        public void Startup_ExecutorAndBackChannel_BothRunning()
        {
            var config = GivenAMinimalConfiguration();
            this.GivenAStartedBackChannelHost(config);
            this.GivenAStartedExecutor(config);

            var statusResponse = new HttpClient().GetAsync(config.BackendAddress + "/fex/jobrun/524868244").Result;

            Assert.AreEqual(HttpStatusCode.NotFound, statusResponse.StatusCode);
        }

        [TestMethod]
        public void PlannedJob_AfterStart_WillCallback()
        {
            // Setup
            var config = GivenAMinimalConfiguration();
            config.JobRunnerExeResolver = () => "Jobbr.Server.ForkedExecution.TestRunner.exe";

            this.GivenAStartedBackChannelHost(config);
            var executor = this.GivenAStartedExecutor(config);

            var fakeRun = this.jobRunFakeTuples.CreateFakeJobRun();

            // Act
            executor.OnPlanChanged(new List<PlannedJobRun>(new [] { fakeRun.PlannedJobRun }));

            // Test
            var hasConnected = this.storedProgressUpdates.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.UniqueId].Contains(JobRunStates.Connected), 1000);

            Assert.IsTrue(hasConnected, "The runner executable should connect within 3s");

            // Tearddown: Wait for Failing or Completed state to that the executable is able to exit
            this.storedProgressUpdates.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.UniqueId].Contains(JobRunStates.Failed) || allUpdates[fakeRun.UniqueId].Contains(JobRunStates.Completed), 10000);
        }

        [TestMethod]
        public void RunnerExecutable_WithInvalidJob_ReturnsFailed()
        {
            // Setup
            var config = GivenAMinimalConfiguration();
            config.JobRunnerExeResolver = () => "Jobbr.Server.ForkedExecution.TestRunner.exe";
            
            this.GivenAStartedBackChannelHost(config);
            var executor = this.GivenAStartedExecutor(config);

            var fakeRun = this.jobRunFakeTuples.CreateFakeJobRun();
            fakeRun.JobRunInfo.Type = "NotExistentClrType";

            // Act
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            // Test
            var hasCompleted = this.storedProgressUpdates.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.UniqueId].Contains(JobRunStates.Failed), 3000);

            Assert.IsTrue(hasCompleted, "The runner executable should fail, but did not within 3s");
            Assert.IsTrue(this.storedProgressUpdates.AllStatusUpdates[fakeRun.UniqueId].Contains(JobRunStates.Failed), "There should be a failed state for this jobRun");
        }

        [TestMethod]
        public void RunnerExecutable_GetsValidJob_ReturnsCompleted()
        {
            // Setup
            var config = GivenAMinimalConfiguration();
            config.JobRunnerExeResolver = () => "Jobbr.Server.ForkedExecution.TestRunner.exe";

            this.GivenAStartedBackChannelHost(config);
            var executor = this.GivenAStartedExecutor(config);

            var fakeRun = this.jobRunFakeTuples.CreateFakeJobRun();
            fakeRun.JobRunInfo.Type = "SimpleJob";

            // Act
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            // Test
            var hasCompleted = this.storedProgressUpdates.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.UniqueId].Contains(JobRunStates.Completed), 3000);

            Assert.IsTrue(hasCompleted, "The runner executable should completed, but did not within 3s");
            Assert.IsTrue(this.storedProgressUpdates.AllStatusUpdates[fakeRun.UniqueId].Contains(JobRunStates.Completed), "There should be a completed state for this jobRun");
        }

        [TestMethod]
        public void RunnerExecutable_JobWithArtefacts_UploadsFiles()
        {
            // Setup
            var config = GivenAMinimalConfiguration();
            config.JobRunnerExeResolver = () => "Jobbr.Server.ForkedExecution.TestRunner.exe";

            this.GivenAStartedBackChannelHost(config);
            var executor = this.GivenAStartedExecutor(config);

            var fakeRun = this.jobRunFakeTuples.CreateFakeJobRun();
            fakeRun.JobRunInfo.Type = "JobWithArtefacts";

            // Act
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            // Test
            var hasCompleted = this.storedProgressUpdates.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.UniqueId].Contains(JobRunStates.Completed), 3000);

            Assert.IsTrue(hasCompleted, "The runner executable should completed, but did not within 3s");
            Assert.IsTrue(this.storedProgressUpdates.AllUploadedArtefacts[fakeRun.UniqueId].Any(), "It should contain at least one file!");
        }

        [TestMethod]
        public void RunnerExecutable_JobWithProgress_SendsPogress()
        {
            // Setup
            var config = GivenAMinimalConfiguration();
            config.JobRunnerExeResolver = () => "Jobbr.Server.ForkedExecution.TestRunner.exe";

            this.GivenAStartedBackChannelHost(config);
            var executor = this.GivenAStartedExecutor(config);

            var fakeRun = this.jobRunFakeTuples.CreateFakeJobRun();
            fakeRun.JobRunInfo.Type = "JobWithOneProgress";

            // Act
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeRun.PlannedJobRun }));

            // Test
            var hasCompleted = this.storedProgressUpdates.WaitForStatusUpdate(allUpdates => allUpdates[fakeRun.UniqueId].Contains(JobRunStates.Completed), 3000);
            var hadProgress = this.storedProgressUpdates.WaitForProgressUpdate(allUpdates => allUpdates[fakeRun.UniqueId].Count() == 1, 1000);

            Assert.IsTrue(hasCompleted, "The runner executable should completed, but did not within 3s");
            Assert.IsTrue(hadProgress, "There should have been one progress issued");
            Assert.AreEqual(1, this.storedProgressUpdates.AllProgressUpdates[fakeRun.UniqueId].Count, "There should be exact one progress update!");
        }
    }
}
