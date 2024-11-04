using System;
using System.Linq;
using Raven.Client.Indexes;

namespace Jobbr.Storage.RavenDB.Model.Index
{
    public class JobRunIndex : AbstractIndexCreationTask<JobRun>
    {
        public class Result
        {
            public string Id { get; set; }
            public string JobId { get; set; }
            public DateTime PlannedStartDateTimeUtc { get; set; }
            public DateTime? ActualEndDateTimeUtc { get; set; }
            public DateTime? ActualStartDateTimeUtc { get; set; }
            public DateTime? EstimatedEndDateTimeUtc { get; set; }
            public int? Pid { get; set; }
            public double? Progress { get; set; }
            public JobRunStates State { get; set; }
            public long TriggerId { get; set; }
            public string UserId { get; set; }
            public string UserDisplayName { get; set; }
            public string JobParameters { get; set; }
            public string InstanceParameters { get; set; }
        }

        public JobRunIndex()
        {
            Map = jobRuns => from jobRun in jobRuns
                select
                new Result
                {
                    Id = jobRun.Id,
                    PlannedStartDateTimeUtc = jobRun.PlannedStartDateTimeUtc,
                    ActualEndDateTimeUtc = jobRun.ActualEndDateTimeUtc,
                    ActualStartDateTimeUtc = jobRun.ActualStartDateTimeUtc,
                    EstimatedEndDateTimeUtc = jobRun.EstimatedEndDateTimeUtc,
                    JobId = jobRun.Job.Id,
                    Pid = jobRun.Pid,
                    Progress = jobRun.Progress,
                    State = jobRun.State,
                    TriggerId = jobRun.Trigger.Id,
                    UserId = jobRun.UserId,
                    UserDisplayName = jobRun.UserDisplayName,
                    JobParameters = jobRun.JobParameters,
                    InstanceParameters = jobRun.InstanceParameters
                };
        }
    }
}