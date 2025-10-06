using System;
using Jobbr.Client;
using Jobbr.ComponentModel.JobStorage.Model;
using Jobbr.Server.WebAPI.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.WebAPI.Tests
{
    [TestClass]
    public class ClientTests : IntegrationTestBase
    {
        [TestMethod]
        public void RetrievingJobById()
        {
            using (GivenRunningServerWithWebApi())
            {
                var client = new JobbrClient(BackendAddress);

                var job = new Job();
                JobStorage.AddJob(job);

                var jobDto = client.GetJob(job.Id);

                Assert.IsTrue(job.Id > 0);
                Assert.AreEqual(job.Id, jobDto.Id);
            }
        }

        [TestMethod]
        public void Query_Jobs_Total_Items()
        {
            using (GivenRunningServerWithWebApi())
            {
                var client = new JobbrClient(BackendAddress);

                JobStorage.AddJob(new Job { Title = "title1", Type = "Some.Type1", UniqueName = "unique1" });
                JobStorage.AddJob(new Job { Title = "title2", Type = "Some.Type2", UniqueName = "unique2" });
                JobStorage.AddJob(new Job { Title = "title3", Type = "Some.Type3", UniqueName = "unique3" });
                JobStorage.AddJob(new Job { Title = "title4", Type = "Some.Type4", UniqueName = "unique4" });
                JobStorage.AddJob(new Job { Title = "title5", Type = "Some.Type5", UniqueName = "unique5" });

                var result = client.QueryJobs();

                Assert.AreEqual(5, result.TotalItems);
            }
        }

        [TestMethod]
        public void GetInstantTrigger()
        {
            using (GivenRunningServerWithWebApi())
            {
                var client = new JobbrClient(BackendAddress);

                var job = new Job();
                JobStorage.AddJob(job);

                var trigger = new InstantTrigger
                {
                    DelayedMinutes = 1
                };
                JobStorage.AddTrigger(job.Id, trigger);

                var triggerDto = client.GetTriggerById<InstantTriggerDto>(job.Id, trigger.Id);

                Assert.IsNotNull(triggerDto);
                Assert.AreEqual(trigger.Id, triggerDto.Id);
                Assert.AreEqual(trigger.DelayedMinutes, triggerDto.DelayedMinutes);
            }
        }

        [TestMethod]
        public void GetScheduledTrigger()
        {
            using (GivenRunningServerWithWebApi())
            {
                var client = new JobbrClient(BackendAddress);

                var job = new Job();
                JobStorage.AddJob(job);

                var trigger = new ScheduledTrigger
                {
                    StartDateTimeUtc = DateTime.UtcNow.AddMinutes(5)
                };
                JobStorage.AddTrigger(job.Id, trigger);

                var triggerDto = client.GetTriggerById<ScheduledTriggerDto>(job.Id, trigger.Id);

                Assert.IsNotNull(triggerDto);
                Assert.AreEqual(trigger.Id, triggerDto.Id);
                Assert.AreEqual(trigger.StartDateTimeUtc, triggerDto.StartDateTimeUtc);
            }
        }

        [TestMethod]
        public void GetRecurringTrigger()
        {
            using (GivenRunningServerWithWebApi())
            {
                var client = new JobbrClient(BackendAddress);

                var job = new Job();
                JobStorage.AddJob(job);

                var trigger = new RecurringTrigger
                {
                    Definition = "* * * * *",
                    Parameters = "{\"Param1\":\"abc\",\"Param2\":123}"
                };
                JobStorage.AddTrigger(job.Id, trigger);

                var triggerDto = client.GetTriggerById<RecurringTriggerDto>(job.Id, trigger.Id);

                Assert.IsNotNull(triggerDto);
                Assert.AreEqual(trigger.Id, triggerDto.Id);
                Assert.AreEqual(trigger.Definition, triggerDto.Definition);
                Assert.AreEqual(trigger.Parameters, triggerDto.Parameters.ToString());
            }
        }

        [TestMethod]
        public void AddInstantTrigger()
        {
            using (GivenRunningServerWithWebApi())
            {
                var client = new JobbrClient(BackendAddress);

                var job = new Job();
                JobStorage.AddJob(job);

                var trigger = new InstantTriggerDto
                {
                    Comment = "test",
                    DelayedMinutes = 1
                };

                var triggerDto = client.AddTrigger(job.Id, trigger);

                Assert.IsNotNull(triggerDto);
                Assert.IsTrue(triggerDto.Id > 0);
                Assert.AreEqual(trigger.Comment, triggerDto.Comment);
                Assert.AreEqual(trigger.DelayedMinutes, triggerDto.DelayedMinutes);
            }
        }

        [TestMethod]
        public void AddScheduledTrigger()
        {
            using (GivenRunningServerWithWebApi())
            {
                var client = new JobbrClient(BackendAddress);

                var job = new Job();
                JobStorage.AddJob(job);

                var trigger = new ScheduledTriggerDto
                {
                    Comment = "test",
                    StartDateTimeUtc = DateTime.UtcNow.AddMinutes(5)
                };

                var triggerDto = client.AddTrigger(job.Id, trigger);

                Assert.IsNotNull(triggerDto);
                Assert.IsTrue(triggerDto.Id > 0);
                Assert.AreEqual(trigger.Comment, triggerDto.Comment);
                Assert.AreEqual(trigger.StartDateTimeUtc, triggerDto.StartDateTimeUtc);
            }
        }

        [TestMethod]
        public void AddRecurringTrigger()
        {
            using (GivenRunningServerWithWebApi())
            {
                var client = new JobbrClient(BackendAddress);

                var job = new Job();
                JobStorage.AddJob(job);

                var trigger = new RecurringTriggerDto
                {
                    Comment = "test",
                    Definition = "* * * * *"
                };

                var triggerDto = client.AddTrigger(job.Id, trigger);

                Assert.IsNotNull(triggerDto);
                Assert.IsTrue(triggerDto.Id > 0);
                Assert.AreEqual(trigger.Comment, triggerDto.Comment);
                Assert.AreEqual(trigger.Definition, triggerDto.Definition);
            }
        }

        [TestMethod]
        public void GetJobRunById()
        {
            using (GivenRunningServerWithWebApi())
            {
                var client = new JobbrClient(BackendAddress);

                var job = new Job();
                JobStorage.AddJob(job);

                var trigger = new RecurringTrigger();
                JobStorage.AddTrigger(job.Id, trigger);

                var jobRun = new JobRun { Job = new Job { Id = job.Id }, Trigger = new RecurringTrigger { Id = trigger.Id } };
                JobStorage.AddJobRun(jobRun);

                var jobRunDto = client.GetJobRunById(jobRun.Id);

                Assert.IsNotNull(jobRunDto);
                Assert.AreEqual(jobRun.Id, jobRunDto.JobRunId);
                Assert.AreEqual(jobRun.Job.Id, jobRunDto.JobId);
                Assert.AreEqual(jobRun.Trigger.Id, jobRunDto.TriggerId);
            }
        }

        [TestMethod]
        public void GetJobRunsByTriggerId()
        {
            using (GivenRunningServerWithWebApi())
            {
                var client = new JobbrClient(BackendAddress);

                var job = new Job();
                JobStorage.AddJob(job);

                var trigger = new RecurringTrigger();
                JobStorage.AddTrigger(job.Id, trigger);

                var jobRun = new JobRun { Job = new Job { Id = job.Id }, Trigger = new RecurringTrigger { Id = trigger.Id } };
                JobStorage.AddJobRun(jobRun);

                var jobRun2 = new JobRun { Job = new Job { Id = job.Id }, Trigger = new RecurringTrigger { Id = trigger.Id } };
                JobStorage.AddJobRun(jobRun2);

                var jobRuns = client.GetJobRunsByTriggerId(job.Id, trigger.Id);

                Assert.AreEqual(2, jobRuns.TotalItems);
            }
        }

        [TestMethod]
        public void Get_JobRuns_By_State()
        {
            using (GivenRunningServerWithWebApi())
            {
                var client = new JobbrClient(BackendAddress);

                var job = new Job();
                JobStorage.AddJob(job);

                var trigger = new RecurringTrigger();
                JobStorage.AddTrigger(job.Id, trigger);

                var jobRun = new JobRun { Job = new Job { Id = job.Id }, Trigger = new RecurringTrigger { Id = trigger.Id }, State = JobRunStates.Completed };
                JobStorage.AddJobRun(jobRun);

                var jobRun2 = new JobRun { Job = new Job { Id = job.Id }, Trigger = new RecurringTrigger { Id = trigger.Id }, State = JobRunStates.Completed };
                JobStorage.AddJobRun(jobRun2);

                var jobRun3 = new JobRun { Job = new Job { Id = job.Id }, Trigger = new RecurringTrigger { Id = trigger.Id }, State = JobRunStates.Failed };
                JobStorage.AddJobRun(jobRun3);

                var jobRuns = client.QueryJobRunsByState("Completed");

                Assert.AreEqual(2, jobRuns.TotalItems);
            }
        }

        [TestMethod]
        public void Get_JobRuns_By_States()
        {
            using (GivenRunningServerWithWebApi())
            {
                var client = new JobbrClient(BackendAddress);

                var job = new Job();
                JobStorage.AddJob(job);

                var trigger = new RecurringTrigger();
                JobStorage.AddTrigger(job.Id, trigger);

                var jobRun = new JobRun { Job = new Job { Id = job.Id }, Trigger = new RecurringTrigger { Id = trigger.Id }, State = JobRunStates.Completed };
                JobStorage.AddJobRun(jobRun);

                var jobRun2 = new JobRun { Job = new Job { Id = job.Id }, Trigger = new RecurringTrigger { Id = trigger.Id }, State = JobRunStates.Completed };
                JobStorage.AddJobRun(jobRun2);

                var jobRun3 = new JobRun { Job = new Job { Id = job.Id }, Trigger = new RecurringTrigger { Id = trigger.Id }, State = JobRunStates.Failed };
                JobStorage.AddJobRun(jobRun3);

                var jobRun4 = new JobRun { Job = new Job { Id = job.Id }, Trigger = new RecurringTrigger { Id = trigger.Id }, State = JobRunStates.Connected };
                JobStorage.AddJobRun(jobRun4);

                var jobRuns = client.QueryJobRunsByStates("Completed,Connected");

                Assert.AreEqual(3, jobRuns.TotalItems);

                jobRuns = client.QueryJobRunsByStates("Completed,Connected,Failed");

                Assert.AreEqual(4, jobRuns.TotalItems);

                jobRuns = client.QueryJobRunsByStates("Failed");

                Assert.AreEqual(1, jobRuns.TotalItems);
            }
        }

        [TestMethod]
        public void UpdateInstantTrigger()
        {
            using (GivenRunningServerWithWebApi())
            {
                var client = new JobbrClient(BackendAddress);

                var job = new Job();
                JobStorage.AddJob(job);

                var trigger = new InstantTrigger();
                JobStorage.AddTrigger(job.Id, trigger);

                var triggerDto = client.UpdateTrigger(job.Id, new InstantTriggerDto { Id = trigger.Id, IsActive = true });

                Assert.IsNotNull(triggerDto);
                Assert.IsTrue(triggerDto.IsActive);
            }
        }

        [TestMethod]
        public void UpdateScheduledTrigger()
        {
            using (GivenRunningServerWithWebApi())
            {
                var client = new JobbrClient(BackendAddress);

                var job = new Job();
                JobStorage.AddJob(job);

                var trigger = new ScheduledTrigger();
                JobStorage.AddTrigger(job.Id, trigger);

                var triggerDto = client.UpdateTrigger(job.Id, new ScheduledTriggerDto { Id = trigger.Id, IsActive = true });

                Assert.IsNotNull(triggerDto);
                Assert.IsTrue(triggerDto.IsActive);
            }
        }

        [TestMethod]
        public void UpdateRecurringTrigger()
        {
            using (GivenRunningServerWithWebApi())
            {
                const string definition = "0 0 * * 6";

                var client = new JobbrClient(BackendAddress);

                var job = new Job();
                JobStorage.AddJob(job);

                var trigger = new RecurringTrigger
                {
                    Definition = definition,
                    EndDateTimeUtc = DateTime.UtcNow.AddDays(10)
                };

                JobStorage.AddTrigger(job.Id, trigger);

                var triggerDto = client.UpdateTrigger(job.Id, new RecurringTriggerDto { Id = trigger.Id, IsActive = true, EndDateTimeUtc = null });

                Assert.IsNotNull(triggerDto);
                Assert.IsTrue(triggerDto.IsActive);

                var trigger2 = (RecurringTrigger)JobStorage.GetTriggerById(job.Id, trigger.Id);

                Assert.IsTrue(trigger2.IsActive);
                Assert.IsNotNull(trigger2.Definition);
                Assert.AreEqual(definition, trigger2.Definition);
                Assert.IsNull(trigger2.EndDateTimeUtc);
            }
        }

        [TestMethod]
        public void Delete_Job_Run()
        {
            using (GivenRunningServerWithWebApi())
            {
                var client = new JobbrClient(BackendAddress);

                var job = new Job();
                JobStorage.AddJob(job);

                var trigger = new RecurringTrigger();
                JobStorage.AddTrigger(job.Id, trigger);

                var jobRun = new JobRun { Job = new Job { Id = job.Id }, Trigger = new RecurringTrigger { Id = trigger.Id }, State = JobRunStates.Completed };
                JobStorage.AddJobRun(jobRun);

                client.DeleteJobRun(jobRun.Id);

                var jobRun2 = JobStorage.GetJobRunById(jobRun.Id);

                Assert.IsTrue(jobRun2.Deleted);
            }
        }
    }
}
