using Jobbr.ComponentModel.JobStorage.Model;
using Jobbr.Storage.LiteDB.Entities;

namespace Jobbr.Storage.LiteDB.Mapping
{
    public static class JobRunMapping
    {
        public static JobRunEntity ToEntity(this JobRun jobRun)
        {
            return new JobRunEntity
            {
                Id = jobRun.Id,
                JobId = jobRun.Job?.Id ?? 0,
                TriggerId = jobRun.Trigger?.Id ?? 0,
                State = jobRun.State,
                Progress = jobRun.Progress,
                PlannedStartDateTimeUtc = jobRun.PlannedStartDateTimeUtc,
                ActualStartDateTimeUtc = jobRun.ActualStartDateTimeUtc,
                ActualEndDateTimeUtc = jobRun.ActualEndDateTimeUtc,
                EstimatedEndDateTimeUtc = jobRun.EstimatedEndDateTimeUtc,
                JobParameters = jobRun.JobParameters,
                InstanceParameters = jobRun.InstanceParameters,
                Pid = jobRun.Pid,
                Deleted = jobRun.Deleted
            };
        }

        public static JobRun ToModel(this JobRunEntity entity, Job job, JobTriggerBase trigger)
        {
            return new JobRun
            {
                Id = entity.Id,
                Job = job,
                Trigger = trigger,
                State = entity.State,
                Progress = entity.Progress,
                PlannedStartDateTimeUtc = entity.PlannedStartDateTimeUtc,
                ActualStartDateTimeUtc = entity.ActualStartDateTimeUtc,
                ActualEndDateTimeUtc = entity.ActualEndDateTimeUtc,
                EstimatedEndDateTimeUtc = entity.EstimatedEndDateTimeUtc,
                JobParameters = entity.JobParameters,
                InstanceParameters = entity.InstanceParameters,
                Pid = entity.Pid,
                Deleted = entity.Deleted
            };
        }
    }
}