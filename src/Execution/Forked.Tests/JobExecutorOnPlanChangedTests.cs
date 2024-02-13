using System;
using System.Collections.Generic;
using System.Linq;
using Jobbr.ComponentModel.Execution.Model;
using Jobbr.Server.ForkedExecution.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.Server.ForkedExecution.Tests
{
    [TestClass]
    public class JobExecutorOnPlanChangedTests : TestBase
    {
        [TestMethod]
        public void StartEmpty_GetsNewPlanWithOneJob_ExecutesJob()
        {
            // Setup
            var forkedExecutionConfiguration = GivenAMinimalConfiguration();
            var executor = GivenAMockedExecutor(forkedExecutionConfiguration);

            // Act
            var fakeJobRun = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);

            ManualTimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeJobRun.PlannedJobRun, }));

            // Wait
            ProgressChannelStore.WaitForStatusUpdate(updatesFromAllJobs => updatesFromAllJobs[fakeJobRun.Id].Contains(JobRunStates.Starting), 3000);

            // Test
            var allStatesForJob = ProgressChannelStore.AllStatusUpdates[fakeJobRun.Id];

            Assert.AreEqual(1, allStatesForJob.Count, "There should be two transitions instead of a timeout");
            Assert.AreEqual(JobRunStates.Starting, allStatesForJob[0], "The Mock should have issued the state 'Starting'");
        }

        [TestMethod]
        public void StartEmpty_GetsMultipleJobs_ExecutesMultipleJobs()
        {
            // Setup
            var forkedExecutionConfiguration = GivenAMinimalConfiguration();
            var executor = GivenAMockedExecutor(forkedExecutionConfiguration);

            // Act: create job runs & send plan
            var fakeJobRun1 = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            var fakeJobRun2 = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            var fakeJobRun3 = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);

            ManualTimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeJobRun1.PlannedJobRun, fakeJobRun2.PlannedJobRun, fakeJobRun3.PlannedJobRun }));

            // Wait for 3 Jobs with at least 2 updates each
            ProgressChannelStore.WaitForStatusUpdate(updatesFromAllJobs => updatesFromAllJobs.Count == 3 && updatesFromAllJobs.All(kvp => kvp.Value.Contains(JobRunStates.Starting)), 3000);

            // Test
            var allStatesForJob1 = ProgressChannelStore.AllStatusUpdates[fakeJobRun1.Id];
            var allStatesForJob2 = ProgressChannelStore.AllStatusUpdates[fakeJobRun2.Id];
            var allStatesForJob3 = ProgressChannelStore.AllStatusUpdates[fakeJobRun3.Id];

            Assert.AreEqual(3, JobRunContextMockFactory.Count, "The factory should have created only the requested amount of contexts");
            Assert.AreEqual(JobRunStates.Starting, allStatesForJob1[0], "The first state of job1 should be 'Starting'");
            Assert.AreEqual(JobRunStates.Starting, allStatesForJob2[0], "The first state of job2 should be 'Starting'");
            Assert.AreEqual(JobRunStates.Starting, allStatesForJob3[0], "The first state of job3 should be 'Starting'");
        }

        [TestMethod]
        public void ContainsAItemInPlan_GetsAdditional_ExecutesMultipleJobs()
        {
            // Setup
            var forkedExecutionConfiguration = GivenAMinimalConfiguration();
            var executor = GivenAMockedExecutor(forkedExecutionConfiguration);

            // Act: Create & send First Plan
            var fakeJobRun1 = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            var fakeJobRun2 = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);

            ManualTimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeJobRun1.PlannedJobRun }));

            // Act: Second Plan that also contains the first item (would not start the first anymore...)
            ManualTimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeJobRun1.PlannedJobRun, fakeJobRun2.PlannedJobRun }));

            // Wait
            var didStart2Jobs = ProgressChannelStore.WaitForStatusUpdate(updatesFromAllJobs => updatesFromAllJobs.Count == 2 && updatesFromAllJobs.All(kvp => kvp.Value.Contains(JobRunStates.Starting) && kvp.Value.Count == 1), 5000);

            // Test
            var statesPerJobRun = string.Join("\n", ProgressChannelStore.AllStatusUpdates.Select(u => $"- JobRun #{u.Key}, States: {string.Join(",", u.Value)}"));
            Assert.IsTrue(didStart2Jobs, "There should be two jobs that have been started after 3s instead the following states:\n\n" + statesPerJobRun);
        }

        [TestMethod]
        public void StartDateInFuture_GetsChangedToNow_ShouldExecute()
        {
            // Setup
            var forkedExecutionConfiguration = GivenAMinimalConfiguration();
            forkedExecutionConfiguration.MaxConcurrentProcesses = 1;
            var executor = GivenAMockedExecutor(forkedExecutionConfiguration);

            // Act: Create & send first plan
            var fakeJobRun1 = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow.AddDays(1));

            ManualTimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeJobRun1.PlannedJobRun }));

            // Act: Send second plan
            var updatedJobRun1 = new PlannedJobRun { Id = fakeJobRun1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow };

            // Move time a bit up to that the updated jobRun gets executed
            ManualTimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { updatedJobRun1 }));

            // Wait
            var didStartJob = ProgressChannelStore.WaitForStatusUpdate(updatesFromAllJobs => updatesFromAllJobs[fakeJobRun1.Id].Contains(JobRunStates.Starting), 3000);

            // Test
            Assert.IsTrue(didStartJob, "A job should have been starting.");
        }

        [TestMethod]
        public void StartDateInFuture_MoveToFuture_ShouldExecute()
        {
            // Setup
            var forkedExecutionConfiguration = GivenAMinimalConfiguration();
            forkedExecutionConfiguration.MaxConcurrentProcesses = 1;
            var executor = GivenAMockedExecutor(forkedExecutionConfiguration);

            // Act: Create & send first plan
            var fakeJobRun1 = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow.AddDays(1));

            ManualTimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeJobRun1.PlannedJobRun }));

            // Act: Move time to future
            ManualTimeProvider.Set(DateTime.UtcNow.AddDays(1));

            // Force re-evaluation
            PeriodicTimerMock.CallbackOnce();

            // Wait
            var didStartJob = ProgressChannelStore.WaitForStatusUpdate(updatesFromAllJobs => updatesFromAllJobs[fakeJobRun1.Id].Contains(JobRunStates.Starting), 3000);

            // Test
            Assert.IsTrue(didStartJob, "A job should have been starting.");
        }

        [TestMethod]
        public void LimitTo2JobsAndStartWith2_GetsUpdatedPlanWith3Jobs_DoesNotStartThird()
        {
            // Setup
            var forkedExecutionConfiguration = GivenAMinimalConfiguration();

            // Only run 2 jobs at a time
            forkedExecutionConfiguration.MaxConcurrentProcesses = 2;
            var executor = GivenAMockedExecutor(forkedExecutionConfiguration);

            // Act 1: Create & Send only first plan
            var fakeJobRun1 = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            var fakeJobRun2 = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            var fakeJobRun3 = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);

            ManualTimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeJobRun1.PlannedJobRun, fakeJobRun2.PlannedJobRun }));

            // Act 2: Send updated plan
            ManualTimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeJobRun1.PlannedJobRun, fakeJobRun2.PlannedJobRun, fakeJobRun3.PlannedJobRun }));

            // Wait
            ProgressChannelStore.WaitForStatusUpdate(updatesFromAllJobs => updatesFromAllJobs[fakeJobRun1.Id].Count + updatesFromAllJobs[fakeJobRun2.Id].Count == 2, 3000);

            // Test
            Assert.IsFalse(ProgressChannelStore.AllStatusUpdates.ContainsKey(fakeJobRun3.Id), "There should be no updates for the third job");
        }

        [TestMethod]
        public void LimitTo2JobsAndStartWith2_GetsUpdatedPlanWith3Jobs_StartsThirdIfCapacityIsAvailable()
        {
            // Setup
            var forkedExecutionConfiguration = GivenAMinimalConfiguration();

            // Only run 2 jobs at a time
            forkedExecutionConfiguration.MaxConcurrentProcesses = 2;
            var executor = GivenAMockedExecutor(forkedExecutionConfiguration);

            // Act 1: Create & Send only first plan
            var fakeJobRun1 = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            var fakeJobRun2 = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            var fakeJobRun3 = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);

            ManualTimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeJobRun1.PlannedJobRun, fakeJobRun2.PlannedJobRun }));

            // Act 2: Send updated plan
            ManualTimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeJobRun1.PlannedJobRun, fakeJobRun2.PlannedJobRun, fakeJobRun3.PlannedJobRun }));

            // Act 3: Set the first job to completed
            JobRunContextMockFactory[fakeJobRun1.Id].RaiseEnded();

            // Simulate timer
            PeriodicTimerMock.CallbackOnce();

            // Wait (optional)
            ProgressChannelStore.WaitForStatusUpdate(updatesFromAllJobs => updatesFromAllJobs.Count == 3, 3000);

            // Test
            Assert.AreEqual(3, JobRunContextMockFactory.Count, "The third job should have been started after the first one has ended");
        }

        [TestMethod]
        public void StartOverbooked_ReducedPlan_DoesNotStartRemoved()
        {
            // Setup
            var forkedExecutionConfiguration = GivenAMinimalConfiguration();

            // Only run 2 jobs at a time
            forkedExecutionConfiguration.MaxConcurrentProcesses = 2;
            var executor = GivenAMockedExecutor(forkedExecutionConfiguration);

            // Create & Send only first plan
            var fakeJobRun1 = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            var fakeJobRun2 = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);
            var fakeJobRun3 = JobRunFakeTuples.CreateFakeJobRun(DateTime.UtcNow);

            // Increase current time so that the fake jobs are in the past
            ManualTimeProvider.AddSecond();
            executor.OnPlanChanged(new List<PlannedJobRun>(new[] { fakeJobRun1.PlannedJobRun, fakeJobRun2.PlannedJobRun, fakeJobRun3.PlannedJobRun }));

            // Act 2: Send an empty plan
            executor.OnPlanChanged(new List<PlannedJobRun>());

            // Complete 1. Job
            JobRunContextMockFactory[fakeJobRun1.Id].RaiseEnded();

            // Should not have created an additional jobruns
            ManualTimeProvider.AddSecond();

            // Force starting new possible runs
            PeriodicTimerMock.CallbackOnce();

            Assert.AreEqual(2, JobRunContextMockFactory.Count);
        }
    }
}