using Jobbr.ComponentModel.JobStorage.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;
using Shouldly;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace Jobbr.Storage.MsSql.Tests
{
    [TestClass]
    public class MsSqlStorageProviderTests
    {
        private MsSqlStorageProvider _storageProvider;

        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("APPVEYOR") == "True"
            ? "Server=(local)\\SQL2017;Database=master;User ID=sa;Password=Password12!"
            : (Environment.GetEnvironmentVariable("GITHUBACTIONS") == "True" ? "Server=localhost,1433;Database=master;User ID=sa;Password=1StrongPwd!!" : "Data Source=localhost\\sqlexpress;Initial Catalog=JobbrTest;Integrated Security=True");

        [TestInitialize]
        public void SetupDatabaseInstance()
        {
            OrmLiteConfig.BeforeExecFilter = dbCmd => Console.WriteLine(Environment.NewLine + dbCmd.GetDebugString() + Environment.NewLine);

            DropTablesIfExists();

            _storageProvider = new MsSqlStorageProvider(new JobbrMsSqlConfiguration
            {
                ConnectionString = ConnectionString,
                DialectProvider = new SqlServer2017OrmLiteDialectProvider(),
                CreateTablesIfNotExists = true
            });
        }

        private static void DropTablesIfExists()
        {
            var factory = new OrmLiteConnectionFactory(ConnectionString, new SqlServer2017OrmLiteDialectProvider());
            var connection = factory.CreateDbConnection();
            connection.Open();

            if (connection.TableExists<Entities.JobRun>())
            {
                connection.DropTable<Entities.JobRun>();
            }

            if (connection.TableExists<Entities.Trigger>())
            {
                connection.DropTable<Entities.Trigger>();
            }

            if (connection.TableExists<Entities.Job>())
            {
                connection.DropTable<Entities.Job>();
            }

            connection.Close();
        }

        [TestMethod]
        public void Adding_Jobs()
        {
            var job1 = new Job
            {
                UniqueName = "testjob",
                Type = "Jobs.Test"
            };

            var job2 = new Job
            {
                UniqueName = "testjob2",
                Type = "Jobs.Test2"
            };

            _storageProvider.AddJob(job1);
            _storageProvider.AddJob(job2);

            job1.Id.ShouldNotBe(job2.Id);
        }

        [TestMethod]
        public void Get_Non_Existing_Job_By_Unique_Name()
        {
            _storageProvider.GetJobByUniqueName("i-dont-exist").ShouldBeNull();
        }

        [TestMethod]
        public void Get_Job_By_Id()
        {
            var job = new Job
            {
                UniqueName = "testjob",
                Type = "Jobs.Test"
            };

            _storageProvider.AddJob(job);

            var job2 = _storageProvider.GetJobById(job.Id);

            job.Id.ShouldBe(job2.Id);
            job2.UniqueName.ShouldBe("testjob");
            job2.Type.ShouldBe("Jobs.Test");
        }

        [TestMethod]
        public void Get_Non_Existing_Job_By_Id()
        {
            _storageProvider.GetJobById(1).ShouldBeNull();
        }

        [TestMethod]
        public void UniqueName_Is_Unique()
        {
            var job1 = new Job
            {
                UniqueName = "i-am-unique",
            };

            var job2 = new Job
            {
                UniqueName = "i-am-unique",
            };

            _storageProvider.AddJob(job1);

            Should.Throw<SqlException>(() => _storageProvider.AddJob(job2), "SqlException should have been raised");
        }

        [TestMethod]
        public void Query_Job_By_Unique_Name()
        {
            var job = new Job
            {
                UniqueName = "testjob",
                Type = "Jobs.Test"
            };

            _storageProvider.AddJob(job);

            var job2 = _storageProvider.GetJobByUniqueName(job.UniqueName);

            job.Id.ShouldBe(job2.Id);
            job2.UniqueName.ShouldBe("testjob");
            job2.Type.ShouldBe("Jobs.Test");
        }

        [TestMethod]
        public void Query_Job_By_JobTypeFilter()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };
            var job2 = new Job { UniqueName = "testjob2", Type = "Jobs.Test2" };
            var job3 = new Job { UniqueName = "testjob3", Type = "Jobs.Test3" };

            _storageProvider.AddJob(job1);
            _storageProvider.AddJob(job2);
            _storageProvider.AddJob(job3);

            var jobs = _storageProvider.GetJobs(jobTypeFilter: "Jobs.Test2");

            jobs.Items.Count.ShouldBe(1);
            jobs.Items[0].Id.ShouldBe(job2.Id);
        }

        [TestMethod]
        public void Query_Job_By_Query()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };
            var job2 = new Job { UniqueName = "testjob2", Type = "Jobs.Test2" };
            var job3 = new Job { UniqueName = "foo", Type = "AnotherOne" };

            _storageProvider.AddJob(job1);
            _storageProvider.AddJob(job2);
            _storageProvider.AddJob(job3);

            var jobs = _storageProvider.GetJobs(query: "Jobs.");

            jobs.Items.Count.ShouldBe(2);
        }

        [TestMethod]
        public void Query_Jobs_Ordered()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };
            var job2 = new Job { UniqueName = "testjob2", Type = "Jobs.Test2" };
            var job3 = new Job { UniqueName = "testjob3", Type = "Jobs.Test3" };

            _storageProvider.AddJob(job1);
            _storageProvider.AddJob(job2);
            _storageProvider.AddJob(job3);

            var jobs = _storageProvider.GetJobs(sort: new[] { "-Type" });

            jobs.Items.Count.ShouldBe(3);
            jobs.Items[0].Id.ShouldBe(job3.Id);
        }

        [TestMethod]
        public void Query_Jobs_Paged()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };
            var job2 = new Job { UniqueName = "testjob2", Type = "Jobs.Test2" };
            var job3 = new Job { UniqueName = "testjob3", Type = "Jobs.Test3" };

            _storageProvider.AddJob(job1);
            _storageProvider.AddJob(job2);
            _storageProvider.AddJob(job3);

            var jobs = _storageProvider.GetJobs(1, 1);

            jobs.Items.Count.ShouldBe(1);
        }

        [TestMethod]
        public void Query_Jobs_Page_Two()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };
            var job2 = new Job { UniqueName = "testjob2", Type = "Jobs.Test2" };
            var job3 = new Job { UniqueName = "testjob3", Type = "Jobs.Test3" };

            _storageProvider.AddJob(job1);
            _storageProvider.AddJob(job2);
            _storageProvider.AddJob(job3);

            var jobs = _storageProvider.GetJobs(2, 1);

            Assert.AreEqual(1, jobs.Items.Count);
            Assert.AreEqual(job2.Id, jobs.Items[0].Id);

            jobs.Items.Count.ShouldBe(1);
            jobs.Items[0].Id.ShouldBe(job2.Id);
        }

        [TestMethod]
        public void Querying_Only_Active_Triggers()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };
            var job2 = new Job { UniqueName = "testjob2", Type = "Jobs.Test2" };

            _storageProvider.AddJob(job1);
            _storageProvider.AddJob(job2);

            var trigger1 = new InstantTrigger { IsActive = false };
            var trigger2 = new InstantTrigger { IsActive = true };
            var trigger3 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);
            _storageProvider.AddTrigger(job1.Id, trigger2);
            _storageProvider.AddTrigger(job2.Id, trigger3);

            var activeTriggers = _storageProvider.GetActiveTriggers();

            activeTriggers.Items.Count.ShouldBe(2);
        }

        [TestMethod]
        public void Querying_Triggers_Of_Job()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };
            var job2 = new Job { UniqueName = "testjob2", Type = "Jobs.Test2" };

            _storageProvider.AddJob(job1);
            _storageProvider.AddJob(job2);

            var trigger1 = new InstantTrigger();
            var trigger2 = new InstantTrigger();
            var trigger3 = new InstantTrigger();

            _storageProvider.AddTrigger(job1.Id, trigger1);
            _storageProvider.AddTrigger(job1.Id, trigger2);
            _storageProvider.AddTrigger(job2.Id, trigger3);

            var triggersOfJob1 = _storageProvider.GetTriggersByJobId(job1.Id);
            var triggersOfJob2 = _storageProvider.GetTriggersByJobId(job2.Id);

            triggersOfJob1.Items.Count.ShouldBe(2);
            triggersOfJob2.Items.Count.ShouldBe(1);
        }

        [TestMethod]
        public void Active_Triggers_Come_First()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = false };
            var trigger2 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);
            _storageProvider.AddTrigger(job1.Id, trigger2);

            var triggersOfJob1 = _storageProvider.GetTriggersByJobId(job1.Id);

            triggersOfJob1.Items.Count.ShouldBe(2);
        }

        [TestMethod]
        public void Querying_Triggers_Of_Job_Ignoring_Deleted()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger();
            var trigger2 = new InstantTrigger();
            var trigger3 = new InstantTrigger { Deleted = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);
            _storageProvider.AddTrigger(job1.Id, trigger2);
            _storageProvider.AddTrigger(job1.Id, trigger3);

            var triggersOfJob1 = _storageProvider.GetTriggersByJobId(job1.Id);

            triggersOfJob1.Items.Count.ShouldBe(2);
        }

        [TestMethod]
        public void Querying_Triggers_Of_Job_Only_Deleted()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger();
            var trigger2 = new InstantTrigger();
            var trigger3 = new InstantTrigger { Deleted = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);
            _storageProvider.AddTrigger(job1.Id, trigger2);
            _storageProvider.AddTrigger(job1.Id, trigger3);

            var triggersOfJob1 = _storageProvider.GetTriggersByJobId(job1.Id, showDeleted: true);

            triggersOfJob1.Items.Count.ShouldBe(1);
        }

        [TestMethod]
        public void Get_JobRun_By_Id()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1", Parameters = "param", Title = "title" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true, DelayedMinutes = 1337, Parameters = "triggerparams", Comment = "comment", UserDisplayName = "chefkoch", UserId = "ck" };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var jobRun1 = new JobRun { Job = job1, Trigger = trigger1, PlannedStartDateTimeUtc = DateTime.UtcNow, JobParameters = "param", InstanceParameters = "triggerparams" };

            _storageProvider.AddJobRun(jobRun1);

            var jobRun2 = _storageProvider.GetJobRunById(jobRun1.Id);

            jobRun1.Id.ShouldBe(jobRun2.Id);

            jobRun2.Trigger.ShouldBeOfType<InstantTrigger>();
            jobRun2.InstanceParameters.ShouldBe("triggerparams");
            jobRun2.JobParameters.ShouldBe("param");
        }

        [TestMethod]
        public void Query_JobRuns_Page_Two()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var jobRun1 = new JobRun
            {
                Job = new Job { Id = job1.Id },
                Trigger = new InstantTrigger { Id = trigger1.Id },
                PlannedStartDateTimeUtc = DateTime.UtcNow,
            };

            var jobRun2 = new JobRun
            {
                Job = new Job { Id = job1.Id },
                Trigger = new InstantTrigger { Id = trigger1.Id },
                PlannedStartDateTimeUtc = DateTime.UtcNow,
            };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);

            var jobRuns = _storageProvider.GetJobRuns(1, 1);

            jobRuns.Items.Count.ShouldBe(1);

            jobRuns = _storageProvider.GetJobRuns(1, 2);

            jobRuns.Items.Count.ShouldBe(2);
        }

        [TestMethod]
        public void Query_JobRuns_For_Specific_State()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun2 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun3 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Failed };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);

            var jobRuns = _storageProvider.GetJobRunsByState(JobRunStates.Failed);

            jobRuns.Items.Count.ShouldBe(1);
        }

        [TestMethod]
        public void Query_JobRuns_By_JobType()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };
            var job2 = new Job { UniqueName = "testjob2", Type = "Jobs.Test2" };

            _storageProvider.AddJob(job1);
            _storageProvider.AddJob(job2);

            var trigger1 = new InstantTrigger { IsActive = true };
            var trigger2 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);
            _storageProvider.AddTrigger(job2.Id, trigger2);

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow };
            var jobRun2 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow };
            var jobRun3 = new JobRun { Job = new Job { Id = job2.Id }, Trigger = new InstantTrigger { Id = trigger2.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);

            var jobRuns = _storageProvider.GetJobRuns(jobTypeFilter: "Jobs.Test1");

            jobRuns.Items.Count.ShouldBe(2);
        }

        [TestMethod]
        public void Query_JobRuns_By_JobType_Ignoring_Deleted()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };
            var job2 = new Job { UniqueName = "testjob2", Type = "Jobs.Test2" };

            _storageProvider.AddJob(job1);
            _storageProvider.AddJob(job2);

            var trigger1 = new InstantTrigger { IsActive = true };
            var trigger2 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);
            _storageProvider.AddTrigger(job2.Id, trigger2);

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, Deleted = true };
            var jobRun2 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow };
            var jobRun3 = new JobRun { Job = new Job { Id = job2.Id }, Trigger = new InstantTrigger { Id = trigger2.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);

            var jobRuns = _storageProvider.GetJobRuns(jobTypeFilter: "Jobs.Test1");

            jobRuns.Items.Count.ShouldBe(1);
        }

        [TestMethod]
        public void Query_JobRuns_By_UniqueName()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };
            var job2 = new Job { UniqueName = "testjob2", Type = "Jobs.Test2" };

            _storageProvider.AddJob(job1);
            _storageProvider.AddJob(job2);

            var trigger1 = new InstantTrigger { IsActive = true };
            var trigger2 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);
            _storageProvider.AddTrigger(job2.Id, trigger2);

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow };
            var jobRun2 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow };
            var jobRun3 = new JobRun { Job = new Job { Id = job2.Id }, Trigger = new InstantTrigger { Id = trigger2.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);

            var jobRuns = _storageProvider.GetJobRuns(1, 50, null, "testjob2", null, false, null);

            jobRuns.Items.Count.ShouldBe(1);
        }

        [TestMethod]
        public void Query_JobRuns_By_UniqueName_Ignoring_Deleted()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };
            var job2 = new Job { UniqueName = "testjob2", Type = "Jobs.Test2" };

            _storageProvider.AddJob(job1);
            _storageProvider.AddJob(job2);

            var trigger1 = new InstantTrigger { IsActive = true };
            var trigger2 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);
            _storageProvider.AddTrigger(job2.Id, trigger2);

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow };
            var jobRun2 = new JobRun { Job = new Job { Id = job2.Id }, Trigger = new InstantTrigger { Id = trigger2.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow };
            var jobRun3 = new JobRun { Job = new Job { Id = job2.Id }, Trigger = new InstantTrigger { Id = trigger2.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow };
            var jobRun4 = new JobRun { Job = new Job { Id = job2.Id }, Trigger = new InstantTrigger { Id = trigger2.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, Deleted = true };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);
            _storageProvider.AddJobRun(jobRun4);

            var jobRuns = _storageProvider.GetJobRuns(1, 50, null, "testjob2", null, false, null);

            jobRuns.Items.Count.ShouldBe(2);
        }

        [TestMethod]
        public void Query_JobRuns_For_Specific_State_Paged()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun2 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun3 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Failed };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);

            var jobRuns = _storageProvider.GetJobRunsByState(JobRunStates.Completed, 1, 1);

            jobRuns.Items.Count.ShouldBe(1);

            jobRuns = _storageProvider.GetJobRunsByState(JobRunStates.Completed, 1, 2);

            jobRuns.Items.Count.ShouldBe(2);
        }

        [TestMethod]
        public void Query_JobRuns_For_Specific_States_Paged()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun2 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun3 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Failed };
            var jobRun4 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Connected };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);
            _storageProvider.AddJobRun(jobRun4);

            var jobRuns = _storageProvider.GetJobRunsByStates(new[] { JobRunStates.Failed, JobRunStates.Connected });

            jobRuns.Items.Count.ShouldBe(2);

            jobRuns = _storageProvider.GetJobRunsByStates(new[] { JobRunStates.Completed });

            jobRuns.Items.Count.ShouldBe(2);

            jobRuns = _storageProvider.GetJobRunsByStates(new[] { JobRunStates.Failed, JobRunStates.Completed });

            jobRuns.Items.Count.ShouldBe(3);
        }

        [TestMethod]
        public void Get_JobRuns_By_Trigger_Id()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun2 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun3 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Failed };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);

            var jobRuns = _storageProvider.GetJobRunsByTriggerId(job1.Id, trigger1.Id);

            jobRuns.Items.Count.ShouldBe(3);
        }

        [TestMethod]
        public void Get_JobRuns_By_Trigger_Id_Ignoring_Deleted()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun2 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun3 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Failed };
            var jobRun4 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Failed, Deleted = true };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);
            _storageProvider.AddJobRun(jobRun4);

            var jobRuns = _storageProvider.GetJobRunsByTriggerId(job1.Id, trigger1.Id);

            jobRuns.Items.Count.ShouldBe(3);
        }

        [TestMethod]
        public void Get_JobRuns_By_Trigger_Id_Only_Deleted()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun2 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun3 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Failed };
            var jobRun4 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Failed, Deleted = true };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);
            _storageProvider.AddJobRun(jobRun4);

            var jobRuns = _storageProvider.GetJobRunsByTriggerId(job1.Id, trigger1.Id, showDeleted: true);

            jobRuns.Items.Count.ShouldBe(1);

            jobRuns.Items.First().Id.ShouldBe(jobRun4.Id);
        }

        [TestMethod]
        public void Get_JobRuns_By_Trigger_Id_Paged()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun2 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun3 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Failed };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);

            var jobRuns = _storageProvider.GetJobRunsByTriggerId(job1.Id, trigger1.Id, 1, 2);

            jobRuns.Items.Count.ShouldBe(2);
        }

        [TestMethod]
        public void Get_JobRuns_By_UserDisplayName()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true, UserDisplayName = "chefkoch" };
            var trigger2 = new InstantTrigger { IsActive = true, UserDisplayName = "foo" };

            _storageProvider.AddTrigger(job1.Id, trigger1);
            _storageProvider.AddTrigger(job1.Id, trigger2);

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun2 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun3 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger2.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);

            var jobRuns = _storageProvider.GetJobRunsByUserDisplayName("chefkoch");

            jobRuns.Items.Count.ShouldBe(2);
        }

        [TestMethod]
        public void Get_JobRuns_by_UserDisplayName()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true, UserDisplayName = "chefkoch" };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun2 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun3 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);

            var jobRuns = _storageProvider.GetJobRunsByUserDisplayName("chefkoch", 1, 2);

            jobRuns.Items.Count.ShouldBe(2);
        }

        [TestMethod]
        public void Get_JobRuns_By_UserId()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true, UserId = "ozu" };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun2 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun3 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);

            var jobRuns = _storageProvider.GetJobRunsByUserId("ozu");

            jobRuns.Items.Count.ShouldBe(3);
        }

        [TestMethod]
        public void Get_JobRuns_By_UserId_Paged()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true, UserId = "ozu" };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun2 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
            var jobRun3 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);

            var jobRuns = _storageProvider.GetJobRunsByUserId("ozu", 1, 2);

            jobRuns.Items.Count.ShouldBe(2);
        }

        [TestMethod]
        public void Disable_Trigger()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            _storageProvider.DisableTrigger(job1.Id, trigger1.Id);

            var triggerFromDb = _storageProvider.GetTriggerById(job1.Id, trigger1.Id);

            triggerFromDb.IsActive.ShouldBeFalse();
        }

        [TestMethod]
        public void Enabling_Trigger()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = false };

            _storageProvider.AddTrigger(job1.Id, trigger1);
            _storageProvider.EnableTrigger(job1.Id, trigger1.Id);

            var triggerFromDb = _storageProvider.GetTriggerById(job1.Id, trigger1.Id);

            triggerFromDb.IsActive.ShouldBeTrue();
        }

        [TestMethod]
        public void Get_Trigger_By_Id()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1", Parameters = "param", Title = "title" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true, DelayedMinutes = 1337, Parameters = "triggerparams", Comment = "comment", UserDisplayName = "chefkoch", UserId = "ck" };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var trigger2 = _storageProvider.GetTriggerById(job1.Id, trigger1.Id);

            trigger2.Id.ShouldBe(trigger1.Id);
            trigger2.JobId.ShouldBe(job1.Id);
            trigger2.Parameters.ShouldBe("triggerparams");
            trigger2.ShouldBeOfType<InstantTrigger>();
        }

        [TestMethod]
        public void Get_Non_Existing_Trigger_By_Id()
        {
            _storageProvider.GetTriggerById(1, 1).ShouldBeNull();
        }

        [TestMethod]
        public void Get_Last_JobRun_By_TriggerId()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var now = DateTime.UtcNow;

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = now, ActualStartDateTimeUtc = now, State = JobRunStates.Completed };
            var jobRun2 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = now.AddMinutes(1), ActualStartDateTimeUtc = now.AddMinutes(1), State = JobRunStates.Completed };
            var jobRun3 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = now.AddMinutes(2), ActualStartDateTimeUtc = now.AddMinutes(2), State = JobRunStates.Completed };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);

            var lastJobRun = _storageProvider.GetLastJobRunByTriggerId(job1.Id, trigger1.Id, now.AddSeconds(30));

            lastJobRun.Id.ShouldBe(jobRun2.Id);
        }

        [TestMethod]
        public void Get_Next_JobRun_By_TriggerId()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var now = DateTime.UtcNow;

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = now, ActualStartDateTimeUtc = now, State = JobRunStates.Completed };
            var jobRun2 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = now.AddMinutes(1), ActualStartDateTimeUtc = now.AddMinutes(1), State = JobRunStates.Completed };
            var jobRun3 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = now.AddMinutes(2), State = JobRunStates.Scheduled };

            _storageProvider.AddJobRun(jobRun1);
            _storageProvider.AddJobRun(jobRun2);
            _storageProvider.AddJobRun(jobRun3);

            var nextJobRun = _storageProvider.GetNextJobRunByTriggerId(job1.Id, trigger1.Id, now.AddMinutes(1));

            nextJobRun.Id.ShouldBe(jobRun3.Id);
        }

        [TestMethod]
        public void Updating_Progress()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var now = DateTime.UtcNow;

            var jobRun1 = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger1.Id }, PlannedStartDateTimeUtc = now, ActualStartDateTimeUtc = now, State = JobRunStates.Completed };

            _storageProvider.AddJobRun(jobRun1);

            _storageProvider.UpdateProgress(jobRun1.Id, 50);

            var jobRun2 = _storageProvider.GetJobRunById(jobRun1.Id);

            jobRun2.Progress.ShouldBe(50);
        }

        [TestMethod]
        public void Updating_Job()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            job1.UniqueName = "test-uniquename";
            job1.Title = "test-title";
            job1.Type = "test-type";
            job1.Parameters = "test-parameters";

            _storageProvider.Update(job1);

            var job1Reloaded = _storageProvider.GetJobById(job1.Id);

            job1Reloaded.UniqueName.ShouldBe("test-uniquename");
            job1Reloaded.Title.ShouldBe("test-title");
            job1Reloaded.Type.ShouldBe("test-type");
            job1Reloaded.Parameters.ShouldBe("test-parameters");
        }

        [TestMethod]
        public void Updating_InstantTrigger()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger);

            var jobRun = new JobRun { Job = new Job { Id = job1.Id }, Trigger = new InstantTrigger { Id = trigger.Id }, PlannedStartDateTimeUtc = DateTime.UtcNow };

            _storageProvider.AddJobRun(jobRun);

            var newPlannedStartDate = DateTime.UtcNow;
            var newActualStartDate = newPlannedStartDate.AddSeconds(1);
            var newEstimatedStartDate = newPlannedStartDate.AddMilliseconds(1);
            var newActualEndDate = newPlannedStartDate.AddMinutes(1);

            jobRun.JobParameters = "test-jobparameters";
            jobRun.InstanceParameters = "test-instanceparameters";
            jobRun.PlannedStartDateTimeUtc = newPlannedStartDate;
            jobRun.ActualStartDateTimeUtc = newActualStartDate;
            jobRun.EstimatedEndDateTimeUtc = newEstimatedStartDate;
            jobRun.ActualEndDateTimeUtc = newActualEndDate;

            _storageProvider.Update(jobRun);

            var job1Reloaded = _storageProvider.GetJobRunById(job1.Id);

            job1Reloaded.JobParameters.ShouldBe("test-jobparameters");
            job1Reloaded.InstanceParameters.ShouldBe("test-instanceparameters");
            job1Reloaded.PlannedStartDateTimeUtc.ShouldBe(newPlannedStartDate, TimeSpan.FromSeconds(1));
            job1Reloaded.ActualStartDateTimeUtc.GetValueOrDefault().ShouldBe(newActualStartDate, TimeSpan.FromSeconds(1));
            job1Reloaded.EstimatedEndDateTimeUtc.GetValueOrDefault().ShouldBe(newEstimatedStartDate, TimeSpan.FromSeconds(1));
            job1Reloaded.ActualEndDateTimeUtc.GetValueOrDefault().ShouldBe(newActualEndDate, TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        public void Update_InstantTrigger()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger = new InstantTrigger();

            _storageProvider.AddTrigger(job1.Id, trigger);

            var trigger2 = (InstantTrigger)_storageProvider.GetTriggerById(job1.Id, trigger.Id);
            trigger2.Comment = "bla";
            trigger2.IsActive = true;
            trigger2.Parameters = "test-parameters";
            trigger2.UserId = "ozu";
            trigger2.DelayedMinutes = 5;

            _storageProvider.Update(job1.Id, trigger2);

            var trigger2Reloaded = (InstantTrigger)_storageProvider.GetTriggerById(job1.Id, trigger2.Id);

            trigger2Reloaded.Comment.ShouldBe("bla");
            trigger2Reloaded.IsActive.ShouldBeTrue();
            trigger2Reloaded.Parameters.ShouldBe("test-parameters");
            trigger2Reloaded.UserId.ShouldBe("ozu");
            trigger2Reloaded.DelayedMinutes.ShouldBe(5);
        }

        [TestMethod]
        public void Updating_ScheduledTrigger()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger = new ScheduledTrigger { StartDateTimeUtc = DateTime.UtcNow };

            _storageProvider.AddTrigger(job1.Id, trigger);

            var trigger2 = (ScheduledTrigger)_storageProvider.GetTriggerById(job1.Id, trigger.Id);

            var startDateTime = DateTime.UtcNow.AddHours(5);

            trigger2.Comment = "bla";
            trigger2.IsActive = true;
            trigger2.Parameters = "test-parameters";
            trigger2.UserId = "ozu";
            trigger2.StartDateTimeUtc = startDateTime;

            _storageProvider.Update(job1.Id, trigger2);

            var trigger2Reloaded = (ScheduledTrigger)_storageProvider.GetTriggerById(job1.Id, trigger2.Id);

            trigger2Reloaded.Comment.ShouldBe("bla");
            trigger2Reloaded.IsActive.ShouldBeTrue();
            trigger2Reloaded.Parameters.ShouldBe("test-parameters");
            trigger2Reloaded.UserId.ShouldBe("ozu");
            trigger2Reloaded.StartDateTimeUtc.ShouldBe(startDateTime, TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        public void Updating_RecurringTrigger()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger = new RecurringTrigger();

            _storageProvider.AddTrigger(job1.Id, trigger);

            var trigger2 = (RecurringTrigger)_storageProvider.GetTriggerById(job1.Id, trigger.Id);

            var startDateTime = DateTime.UtcNow.AddHours(5);
            var endDateTime = DateTime.UtcNow.AddHours(7);

            trigger2.Comment = "bla";
            trigger2.IsActive = true;
            trigger2.Parameters = "test-parameters";
            trigger2.UserId = "ozu";
            trigger2.StartDateTimeUtc = startDateTime;
            trigger2.Definition = "* * * * *";
            trigger2.EndDateTimeUtc = endDateTime;
            trigger2.NoParallelExecution = true;

            _storageProvider.Update(job1.Id, trigger2);

            var trigger2Reloaded = (RecurringTrigger)_storageProvider.GetTriggerById(job1.Id, trigger2.Id);

            trigger2Reloaded.Comment.ShouldBe("bla");
            trigger2Reloaded.IsActive.ShouldBeTrue();
            trigger2Reloaded.Parameters.ShouldBe("test-parameters");
            trigger2Reloaded.UserId.ShouldBe("ozu");
            trigger2Reloaded.StartDateTimeUtc.GetValueOrDefault().ShouldBe(startDateTime, TimeSpan.FromSeconds(1));
            trigger2Reloaded.EndDateTimeUtc.GetValueOrDefault().ShouldBe(endDateTime, TimeSpan.FromSeconds(1));
            trigger2Reloaded.Definition.ShouldBe("* * * * *");
            trigger2Reloaded.NoParallelExecution.ShouldBeTrue();
        }

        [TestMethod]
        public void Check_Availability()
        {
            Assert.IsTrue(_storageProvider.IsAvailable());
        }

        [TestMethod]
        public void Get_Jobs_Count()
        {
            var job = new Job
            {
                UniqueName = "testjob",
                Type = "Jobs.Test"
            };

            _storageProvider.AddJob(job);

            var jobCount = _storageProvider.GetJobsCount();

            jobCount.ShouldBe(1);
        }

        [TestMethod]
        public void Job_Count_Does_Not_Count_Deleted_Jobs()
        {
            var job = new Job
            {
                UniqueName = "testjob",
                Type = "Jobs.Test"
            };

            _storageProvider.AddJob(job);

            var jobCount = _storageProvider.GetJobsCount();

            job.Deleted = true;

            _storageProvider.Update(job);

            var jobCount2 = _storageProvider.GetJobsCount();

            jobCount.ShouldBeGreaterThan(jobCount2);
        }

        [TestMethod]
        public void Get_Only_Deleted_Jobs()
        {
            var job = new Job
            {
                UniqueName = "testjob",
                Type = "Jobs.Test",
                Deleted = false
            };

            var deletedJob = new Job
            {
                UniqueName = "testjob2",
                Type = "Jobs.Test2",
                Deleted = true
            };

            _storageProvider.AddJob(job);
            _storageProvider.AddJob(deletedJob);

            var jobs = _storageProvider.GetJobs(showDeleted: true);

            jobs.TotalItems.ShouldBe(1);

            var jobFromStorage = jobs.Items.First();
            jobFromStorage.Id.ShouldBe(deletedJob.Id);
            jobFromStorage.UniqueName.ShouldBe(deletedJob.UniqueName);
            jobFromStorage.Type.ShouldBe(deletedJob.Type);
        }

        [TestMethod]
        public void Get_Only_Not_Deleted_Jobs()
        {
            var job = new Job
            {
                UniqueName = "testjob",
                Type = "Jobs.Test",
                Deleted = false
            };

            var deletedJob = new Job
            {
                UniqueName = "testjob2",
                Type = "Jobs.Test2",
                Deleted = true
            };

            _storageProvider.AddJob(job);
            _storageProvider.AddJob(deletedJob);

            var jobs = _storageProvider.GetJobs();

            jobs.TotalItems.ShouldBe(1);

            var jobFromStorage = jobs.Items.First();
            jobFromStorage.Id.ShouldBe(job.Id);
            jobFromStorage.UniqueName.ShouldBe(job.UniqueName);
            jobFromStorage.Type.ShouldBe(job.Type);
        }

        [TestMethod]
        public void JobRun_With_Instant_Trigger_Is_Deleted_When_Retention_Is_Applied()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var existingJobRun = new JobRun
            {
                Job = new Job { Id = job1.Id },
                Trigger = new InstantTrigger { Id = trigger1.Id },
                PlannedStartDateTimeUtc = DateTime.UtcNow.AddDays(-31),
                State = JobRunStates.Completed,
            };

            _storageProvider.AddJobRun(existingJobRun);

            var existingJobRunFromDb = _storageProvider.GetJobRunById(existingJobRun.Id);
            var existingTriggerFromDb = _storageProvider.GetTriggerById(job1.Id, trigger1.Id);

            existingJobRunFromDb.ShouldNotBeNull();
            existingTriggerFromDb.ShouldNotBeNull();

            _storageProvider.ApplyRetention(DateTimeOffset.UtcNow.AddDays(-30));

            existingJobRunFromDb = _storageProvider.GetJobRunById(existingJobRun.Id);

            existingJobRunFromDb.ShouldBeNull();

            existingTriggerFromDb = _storageProvider.GetTriggerById(job1.Id, trigger1.Id);

            existingTriggerFromDb.ShouldBeNull();
        }

        [TestMethod]
        public void JobRun_With_Scheduled_Trigger_Is_Deleted_When_Retention_Is_Applied()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new ScheduledTrigger { IsActive = true, StartDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromDays(31)) };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var existingJobRun = new JobRun
            {
                Job = new Job { Id = job1.Id },
                Trigger = new InstantTrigger { Id = trigger1.Id },
                PlannedStartDateTimeUtc = DateTime.UtcNow.AddDays(-31),
                State = JobRunStates.Completed,
            };

            _storageProvider.AddJobRun(existingJobRun);

            var existingJobRunFromDb = _storageProvider.GetJobRunById(existingJobRun.Id);
            var existingTriggerFromDb = _storageProvider.GetTriggerById(job1.Id, trigger1.Id);

            existingJobRunFromDb.ShouldNotBeNull();
            existingTriggerFromDb.ShouldNotBeNull();

            _storageProvider.ApplyRetention(DateTimeOffset.UtcNow.AddDays(-30));

            existingJobRunFromDb = _storageProvider.GetJobRunById(existingJobRun.Id);

            existingJobRunFromDb.ShouldBeNull();

            existingTriggerFromDb = _storageProvider.GetTriggerById(job1.Id, trigger1.Id);

            existingTriggerFromDb.ShouldBeNull();
        }

        [TestMethod]
        public void JobRun_With_Recurring_Trigger_Is_Deleted_When_Retention_Is_Applied()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new RecurringTrigger { IsActive = true, StartDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromDays(31)), Definition = "* * * * *" };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var existingJobRun = new JobRun
            {
                Job = new Job { Id = job1.Id },
                Trigger = new InstantTrigger { Id = trigger1.Id },
                PlannedStartDateTimeUtc = DateTime.UtcNow.AddDays(-31),
                State = JobRunStates.Completed,
            };

            _storageProvider.AddJobRun(existingJobRun);

            var existingJobRunFromDb = _storageProvider.GetJobRunById(existingJobRun.Id);
            var existingTriggerFromDb = _storageProvider.GetTriggerById(job1.Id, trigger1.Id);

            existingJobRunFromDb.ShouldNotBeNull();
            existingTriggerFromDb.ShouldNotBeNull();

            _storageProvider.ApplyRetention(DateTimeOffset.UtcNow.AddDays(-30));

            existingJobRunFromDb = _storageProvider.GetJobRunById(existingJobRun.Id);

            existingJobRunFromDb.ShouldBeNull();

            existingTriggerFromDb = _storageProvider.GetTriggerById(job1.Id, trigger1.Id);

            existingTriggerFromDb.ShouldNotBeNull();
        }

        [TestMethod]
        public void Only_Jobs_After_Deadline_Are_Deleted()
        {
            var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

            _storageProvider.AddJob(job1);

            var trigger1 = new InstantTrigger { IsActive = true };

            _storageProvider.AddTrigger(job1.Id, trigger1);

            var existingJobRun = new JobRun
            {
                Job = new Job { Id = job1.Id },
                Trigger = new InstantTrigger { Id = trigger1.Id },
                PlannedStartDateTimeUtc = DateTime.UtcNow,
                State = JobRunStates.Completed,
                ActualEndDateTimeUtc = DateTime.UtcNow.AddDays(-29)
            };

            _storageProvider.AddJobRun(existingJobRun);

            var existingJobRunFromDb = _storageProvider.GetJobRunById(existingJobRun.Id);
            var existingTriggerFromDb = _storageProvider.GetTriggerById(job1.Id, trigger1.Id);

            existingJobRunFromDb.ShouldNotBeNull();
            existingTriggerFromDb.ShouldNotBeNull();

            _storageProvider.ApplyRetention(DateTimeOffset.UtcNow.AddDays(-30));

            existingJobRunFromDb = _storageProvider.GetJobRunById(existingJobRun.Id);

            existingJobRunFromDb.ShouldNotBeNull();

            existingTriggerFromDb = _storageProvider.GetTriggerById(job1.Id, trigger1.Id);

            existingTriggerFromDb.ShouldNotBeNull();
        }
    }
}
