﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.Server.IntegrationTests.Integration.Execution
{
    [TestClass]
    public class JobRunInformationServiceTests : JobRunExecutionTestBase
    {
        [TestMethod]
        public void RunningServer_GetInfoByRandomId_ReturnsNull()
        {
            var result = Services.InformationService.GetByJobRunId(-12);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ExistingJobWithFirstRun_GetInfoById_MatchesConfiguration()
        {
            var job = CreateTestJob();

            var trigger = CreateInstantTrigger(job);

            var createdJobRun = TriggerNewJobRun(trigger);

            var result = Services.InformationService.GetByJobRunId(createdJobRun.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(job.Id, result.JobId);
            Assert.AreEqual(job.Type, result.Type);
            Assert.AreEqual(job.Parameters, result.JobParameters);

            Assert.AreEqual(trigger.Parameters, result.InstanceParameters);
            Assert.AreEqual(trigger.UserId, result.UserId);
            Assert.AreEqual(trigger.UserDisplayName, result.UserDisplayName);

            Assert.AreEqual(job.Parameters, result.JobParameters);
            Assert.AreEqual(createdJobRun.InstanceParameters, result.InstanceParameters);
        }

        [TestMethod]
        public void ExistingJobWithSecondRun_GetInfoById_MatchesConfiguration()
        {
            var job = CreateTestJob();

            // First run
            TriggerNewJobRun(CreateInstantTrigger(job));

            // Second run
            var secondTrigger = CreateInstantTrigger(job);

            var secondJobRun = TriggerNewJobRun(secondTrigger);

            var result = Services.InformationService.GetByJobRunId(secondJobRun.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(job.Id, result.JobId);
            Assert.AreEqual(job.Type, result.Type);
            Assert.AreEqual(job.Parameters, result.JobParameters);

            Assert.AreEqual(secondTrigger.Parameters, result.InstanceParameters);
            Assert.AreEqual(secondTrigger.UserId, result.UserId);
            Assert.AreEqual(secondTrigger.UserDisplayName, result.UserDisplayName);

            Assert.AreEqual(job.Parameters, result.JobParameters);
            Assert.AreEqual(secondJobRun.InstanceParameters, result.InstanceParameters);
        }
    }
}
