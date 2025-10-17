using System;
using System.IO;
using System.Linq;
using Jobbr.ComponentModel.JobStorage.Model;
using Jobbr.Storage.LiteDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Jobbr.Storage.LiteDB.Tests
{
    [TestClass]
    public class LiteDbStorageProviderTests
    {
        private LiteDbStorageProvider _storageProvider;
        private string _tempDatabasePath;

        [TestInitialize]
        public void SetupDatabaseInstance()
        {
            _tempDatabasePath = Path.GetTempFileName();
            File.Delete(_tempDatabasePath); // Delete the temp file so LiteDB can create it

            _storageProvider = new LiteDbStorageProvider(new JobbrLiteDbConfiguration
            {
                ConnectionString = _tempDatabasePath,
                CreateDatabaseIfNotExists = true
            });
        }

        [TestCleanup]
        public void CleanupDatabase()
        {
            _storageProvider?.Dispose();

            if (File.Exists(_tempDatabasePath))
            {
                File.Delete(_tempDatabasePath);
            }
        }

        #region Job Tests

        [TestMethod]
        public void AddJob_ShouldSetIdAndTimestamps()
        {
            var job = new Job
            {
                UniqueName = "testjob",
                Type = "Jobs.Test",
                Title = "Test Job"
            };

            _storageProvider.AddJob(job);

            job.Id.ShouldBeGreaterThan(0);
            job.CreatedDateTimeUtc.ShouldNotBeNull();
            job.UpdatedDateTimeUtc.ShouldNotBeNull();
        }

        [TestMethod]
        public void GetJobById_ShouldReturnCorrectJob()
        {
            var job = new Job
            {
                UniqueName = "testjob",
                Type = "Jobs.Test",
                Title = "Test Job"
            };

            _storageProvider.AddJob(job);
            var retrievedJob = _storageProvider.GetJobById(job.Id);

            retrievedJob.ShouldNotBeNull();
            retrievedJob.UniqueName.ShouldBe(job.UniqueName);
            retrievedJob.Type.ShouldBe(job.Type);
            retrievedJob.Title.ShouldBe(job.Title);
        }

        [TestMethod]
        public void GetJobByUniqueName_ShouldReturnCorrectJob()
        {
            var job = new Job
            {
                UniqueName = "testjob",
                Type = "Jobs.Test",
                Title = "Test Job"
            };

            _storageProvider.AddJob(job);
            var retrievedJob = _storageProvider.GetJobByUniqueName("testjob");

            retrievedJob.ShouldNotBeNull();
            retrievedJob.Id.ShouldBe(job.Id);
            retrievedJob.Type.ShouldBe(job.Type);
        }

        [TestMethod]
        public void UpdateJob_ShouldUpdateTimestamp()
        {
            var job = new Job
            {
                UniqueName = "testjob",
                Type = "Jobs.Test",
                Title = "Test Job"
            };

            _storageProvider.AddJob(job);
            var originalUpdate = job.UpdatedDateTimeUtc;

            System.Threading.Thread.Sleep(10); // Ensure timestamp difference

            job.Title = "Updated Test Job";
            _storageProvider.Update(job);

            job.UpdatedDateTimeUtc.ShouldNotBeNull();
            originalUpdate.ShouldNotBeNull();
            job.UpdatedDateTimeUtc.Value.ShouldBeGreaterThan(originalUpdate.Value);
        }

        [TestMethod]
        public void DeleteJob_ShouldMarkAsDeleted()
        {
            var job = new Job
            {
                UniqueName = "testjob",
                Type = "Jobs.Test",
                Title = "Test Job"
            };

            _storageProvider.AddJob(job);
            _storageProvider.DeleteJob(job.Id);

            var retrievedJob = _storageProvider.GetJobById(job.Id);
            retrievedJob.Deleted.ShouldBeTrue();
        }

        [TestMethod]
        public void GetJobsCount_ShouldReturnCorrectCount()
        {
            var job1 = new Job { UniqueName = "job1", Type = "Jobs.Test1" };
            var job2 = new Job { UniqueName = "job2", Type = "Jobs.Test2" };
            var job3 = new Job { UniqueName = "job3", Type = "Jobs.Test3" };

            _storageProvider.AddJob(job1);
            _storageProvider.AddJob(job2);
            _storageProvider.AddJob(job3);

            _storageProvider.GetJobsCount().ShouldBe(3);

            _storageProvider.DeleteJob(job2.Id);
            _storageProvider.GetJobsCount().ShouldBe(2);
        }

        [TestMethod]
        public void GetJobs_ShouldReturnPagedResults()
        {
            for (int i = 1; i <= 25; i++)
            {
                var job = new Job
                {
                    UniqueName = $"job{i}",
                    Type = "Jobs.Test",
                    Title = $"Test Job {i}"
                };
                _storageProvider.AddJob(job);
            }

            var firstPage = _storageProvider.GetJobs(page: 1, pageSize: 10);
            firstPage.Items.Count.ShouldBe(10);
            firstPage.TotalItems.ShouldBe(25);
            firstPage.Page.ShouldBe(1);
            firstPage.PageSize.ShouldBe(10);

            var secondPage = _storageProvider.GetJobs(page: 2, pageSize: 10);
            secondPage.Items.Count.ShouldBe(10);
            secondPage.Page.ShouldBe(2);

            var thirdPage = _storageProvider.GetJobs(page: 3, pageSize: 10);
            thirdPage.Items.Count.ShouldBe(5);
            thirdPage.Page.ShouldBe(3);
        }

        #endregion

        #region Trigger Tests

        [TestMethod]
        public void AddInstantTrigger_ShouldSetIdAndTimestamp()
        {
            var job = new Job { UniqueName = "testjob", Type = "Jobs.Test" };
            _storageProvider.AddJob(job);

            var trigger = new InstantTrigger
            {
                DelayedMinutes = 5,
                UserId = "user1",
                UserDisplayName = "User One"
            };

            _storageProvider.AddTrigger(job.Id, trigger);

            trigger.Id.ShouldBeGreaterThan(0);
            trigger.JobId.ShouldBe(job.Id);
            trigger.CreatedDateTimeUtc.ShouldNotBe(default);
        }

        [TestMethod]
        public void AddRecurringTrigger_ShouldSetIdAndTimestamp()
        {
            var job = new Job { UniqueName = "testjob", Type = "Jobs.Test" };
            _storageProvider.AddJob(job);

            var trigger = new RecurringTrigger
            {
                Definition = "0 0 * * *", // Daily at midnight
                StartDateTimeUtc = DateTime.UtcNow,
                UserId = "user1",
                UserDisplayName = "User One"
            };

            _storageProvider.AddTrigger(job.Id, trigger);

            trigger.Id.ShouldBeGreaterThan(0);
            trigger.JobId.ShouldBe(job.Id);
            trigger.CreatedDateTimeUtc.ShouldNotBe(default);
        }

        [TestMethod]
        public void AddScheduledTrigger_ShouldSetIdAndTimestamp()
        {
            var job = new Job { UniqueName = "testjob", Type = "Jobs.Test" };
            _storageProvider.AddJob(job);

            var trigger = new ScheduledTrigger
            {
                StartDateTimeUtc = DateTime.UtcNow.AddHours(1),
                UserId = "user1",
                UserDisplayName = "User One"
            };

            _storageProvider.AddTrigger(job.Id, trigger);

            trigger.Id.ShouldBeGreaterThan(0);
            trigger.JobId.ShouldBe(job.Id);
            trigger.CreatedDateTimeUtc.ShouldNotBe(default);
        }

        [TestMethod]
        public void GetTriggerById_ShouldReturnCorrectTrigger()
        {
            var job = new Job { UniqueName = "testjob", Type = "Jobs.Test" };
            _storageProvider.AddJob(job);

            var trigger = new InstantTrigger
            {
                DelayedMinutes = 5,
                UserId = "user1",
                UserDisplayName = "User One"
            };

            _storageProvider.AddTrigger(job.Id, trigger);
            var retrievedTrigger = _storageProvider.GetTriggerById(job.Id, trigger.Id);

            retrievedTrigger.ShouldNotBeNull();
            retrievedTrigger.ShouldBeOfType<InstantTrigger>();
            ((InstantTrigger)retrievedTrigger).DelayedMinutes.ShouldBe(5);
        }

        [TestMethod]
        public void DisableTrigger_ShouldSetIsActiveFalse()
        {
            var job = new Job { UniqueName = "testjob", Type = "Jobs.Test" };
            _storageProvider.AddJob(job);

            var trigger = new InstantTrigger
            {
                DelayedMinutes = 5,
                IsActive = true
            };

            _storageProvider.AddTrigger(job.Id, trigger);
            _storageProvider.DisableTrigger(job.Id, trigger.Id);

            var retrievedTrigger = _storageProvider.GetTriggerById(job.Id, trigger.Id);
            retrievedTrigger.IsActive.ShouldBeFalse();
        }

        [TestMethod]
        public void EnableTrigger_ShouldSetIsActiveTrue()
        {
            var job = new Job { UniqueName = "testjob", Type = "Jobs.Test" };
            _storageProvider.AddJob(job);

            var trigger = new InstantTrigger
            {
                DelayedMinutes = 5,
                IsActive = false
            };

            _storageProvider.AddTrigger(job.Id, trigger);
            _storageProvider.EnableTrigger(job.Id, trigger.Id);

            var retrievedTrigger = _storageProvider.GetTriggerById(job.Id, trigger.Id);
            retrievedTrigger.IsActive.ShouldBeTrue();
        }

        [TestMethod]
        public void DeleteTrigger_ShouldMarkAsDeleted()
        {
            var job = new Job { UniqueName = "testjob", Type = "Jobs.Test" };
            _storageProvider.AddJob(job);

            var trigger = new InstantTrigger { DelayedMinutes = 5 };
            _storageProvider.AddTrigger(job.Id, trigger);
            _storageProvider.DeleteTrigger(job.Id, trigger.Id);

            var retrievedTrigger = _storageProvider.GetTriggerById(job.Id, trigger.Id);
            retrievedTrigger.Deleted.ShouldBeTrue();
        }

        #endregion

        #region JobRun Tests

        [TestMethod]
        public void AddJobRun_ShouldSetId()
        {
            var job = new Job { UniqueName = "testjob", Type = "Jobs.Test" };
            _storageProvider.AddJob(job);

            var trigger = new InstantTrigger { DelayedMinutes = 5 };
            _storageProvider.AddTrigger(job.Id, trigger);

            var jobRun = new JobRun
            {
                Job = job,
                Trigger = trigger,
                State = JobRunStates.Scheduled,
                PlannedStartDateTimeUtc = DateTime.UtcNow
            };

            _storageProvider.AddJobRun(jobRun);

            jobRun.Id.ShouldBeGreaterThan(0);
        }

        [TestMethod]
        public void GetJobRunById_ShouldReturnCorrectJobRun()
        {
            var job = new Job { UniqueName = "testjob", Type = "Jobs.Test" };
            _storageProvider.AddJob(job);

            var trigger = new InstantTrigger { DelayedMinutes = 5 };
            _storageProvider.AddTrigger(job.Id, trigger);

            var jobRun = new JobRun
            {
                Job = job,
                Trigger = trigger,
                State = JobRunStates.Scheduled,
                PlannedStartDateTimeUtc = DateTime.UtcNow
            };

            _storageProvider.AddJobRun(jobRun);
            var retrievedJobRun = _storageProvider.GetJobRunById(jobRun.Id);

            retrievedJobRun.ShouldNotBeNull();
            retrievedJobRun.State.ShouldBe(JobRunStates.Scheduled);
            retrievedJobRun.Job.ShouldNotBeNull();
            retrievedJobRun.Trigger.ShouldNotBeNull();
        }

        [TestMethod]
        public void UpdateJobRun_ShouldPersistChanges()
        {
            var job = new Job { UniqueName = "testjob", Type = "Jobs.Test" };
            _storageProvider.AddJob(job);

            var trigger = new InstantTrigger { DelayedMinutes = 5 };
            _storageProvider.AddTrigger(job.Id, trigger);

            var jobRun = new JobRun
            {
                Job = job,
                Trigger = trigger,
                State = JobRunStates.Scheduled,
                PlannedStartDateTimeUtc = DateTime.UtcNow
            };

            _storageProvider.AddJobRun(jobRun);

            jobRun.State = JobRunStates.Processing;
            jobRun.ActualStartDateTimeUtc = DateTime.UtcNow;
            _storageProvider.Update(jobRun);

            var retrievedJobRun = _storageProvider.GetJobRunById(jobRun.Id);
            retrievedJobRun.State.ShouldBe(JobRunStates.Processing);
            retrievedJobRun.ActualStartDateTimeUtc.ShouldNotBeNull();
        }

        [TestMethod]
        public void UpdateProgress_ShouldUpdateProgressField()
        {
            var job = new Job { UniqueName = "testjob", Type = "Jobs.Test" };
            _storageProvider.AddJob(job);

            var trigger = new InstantTrigger { DelayedMinutes = 5 };
            _storageProvider.AddTrigger(job.Id, trigger);

            var jobRun = new JobRun
            {
                Job = job,
                Trigger = trigger,
                State = JobRunStates.Processing,
                PlannedStartDateTimeUtc = DateTime.UtcNow
            };

            _storageProvider.AddJobRun(jobRun);
            _storageProvider.UpdateProgress(jobRun.Id, 0.5);

            var retrievedJobRun = _storageProvider.GetJobRunById(jobRun.Id);
            retrievedJobRun.Progress.ShouldBe(0.5);
        }

        [TestMethod]
        public void GetJobRunsByState_ShouldReturnCorrectJobRuns()
        {
            var job = new Job { UniqueName = "testjob", Type = "Jobs.Test" };
            _storageProvider.AddJob(job);

            var trigger = new InstantTrigger { DelayedMinutes = 5 };
            _storageProvider.AddTrigger(job.Id, trigger);

            // Add multiple job runs with different states
            for (int i = 0; i < 5; i++)
            {
                var jobRun = new JobRun
                {
                    Job = job,
                    Trigger = trigger,
                    State = i % 2 == 0 ? JobRunStates.Scheduled : JobRunStates.Processing,
                    PlannedStartDateTimeUtc = DateTime.UtcNow.AddMinutes(i)
                };
                _storageProvider.AddJobRun(jobRun);
            }

            var scheduledRuns = _storageProvider.GetJobRunsByState(JobRunStates.Scheduled);
            var processingRuns = _storageProvider.GetJobRunsByState(JobRunStates.Processing);

            scheduledRuns.Items.Count.ShouldBe(3);
            processingRuns.Items.Count.ShouldBe(2);
        }

        #endregion

        [TestMethod]
        public void IsAvailable_ShouldReturnTrue()
        {
            _storageProvider.IsAvailable().ShouldBeTrue();
        }
    }
}