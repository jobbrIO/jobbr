using Jobbr.ComponentModel.JobStorage.Model;

namespace Jobbr.Storage.MsSql.Mapping
{
    public static class JobRunMapping
    {
        public static JobRun ToModel(this Entities.JobRunInfo entity)
        {
            var jobrun = new JobRun
            {
                Id = entity.Id,
                ActualEndDateTimeUtc = entity.ActualEndDateTimeUtc,
                ActualStartDateTimeUtc = entity.ActualStartDateTimeUtc,
                EstimatedEndDateTimeUtc = entity.EstimatedEndDateTimeUtc,
                InstanceParameters = entity.InstanceParameters,
                JobParameters = entity.JobParameters,
                Pid = entity.Pid,
                PlannedStartDateTimeUtc = entity.PlannedStartDateTimeUtc,
                Progress = entity.Progress,
                State = entity.State,
                Deleted = entity.Deleted,
                Job = new Job
                {
                    Id = entity.JobId,
                    CreatedDateTimeUtc = entity.JobCreatedDateTimeUtc,
                    UniqueName = entity.JobUniqueName,
                    Type = entity.JobType,
                    Parameters = entity.JobParameters,
                    UpdatedDateTimeUtc = entity.JobUpdatedDateTimeUtc,
                    Title = entity.JobTitle,
                },
            };

            if (entity.TriggerType == Entities.TriggerType.InstantTrigger)
            {
                jobrun.Trigger = new InstantTrigger
                {
                    Id = entity.TriggerId,
                    UserDisplayName = entity.TriggerUserDisplayName,
                    JobId = entity.JobId,
                    CreatedDateTimeUtc = entity.TriggerCreatedDateTimeUtc,
                    Parameters = entity.TriggerParameters,
                    DelayedMinutes = entity.TriggerDelayedMinutes,
                    UserId = entity.TriggerUserId,
                    IsActive = entity.TriggerIsActive,
                    Comment = entity.TriggerComment,
                    Deleted = entity.Deleted,
                };
            }
            else if (entity.TriggerType == Entities.TriggerType.RecurringTrigger)
            {
                jobrun.Trigger = new RecurringTrigger
                {
                    Id = entity.TriggerId,
                    UserDisplayName = entity.TriggerUserDisplayName,
                    JobId = entity.JobId,
                    CreatedDateTimeUtc = entity.TriggerCreatedDateTimeUtc,
                    Parameters = entity.TriggerParameters,
                    UserId = entity.TriggerUserId,
                    IsActive = entity.TriggerIsActive,
                    Comment = entity.TriggerComment,
                    EndDateTimeUtc = entity.TriggerEndDateTimeUtc,
                    StartDateTimeUtc = entity.TriggerStartDateTimeUtc,
                    Definition = entity.TriggerDefinition,
                    NoParallelExecution = entity.TriggerNoParallelExecution,
                    Deleted = entity.Deleted,
                };
            }
            else if (entity.TriggerType == Entities.TriggerType.ScheduledTrigger)
            {
                jobrun.Trigger = new RecurringTrigger
                {
                    Id = entity.TriggerId,
                    UserDisplayName = entity.TriggerUserDisplayName,
                    JobId = entity.JobId,
                    CreatedDateTimeUtc = entity.TriggerCreatedDateTimeUtc,
                    Parameters = entity.TriggerParameters,
                    UserId = entity.TriggerUserId,
                    IsActive = entity.TriggerIsActive,
                    Comment = entity.TriggerComment,
                    EndDateTimeUtc = entity.TriggerEndDateTimeUtc,
                    StartDateTimeUtc = entity.TriggerStartDateTimeUtc,
                    Definition = entity.TriggerDefinition,
                    NoParallelExecution = entity.TriggerNoParallelExecution,
                    Deleted = entity.Deleted,
                };
            }

            return jobrun;
        }

        public static Entities.JobRun ToEntity(this JobRun model)
        {
            var entity = new Entities.JobRun
            {
                Id = model.Id,
                ActualEndDateTimeUtc = model.ActualEndDateTimeUtc,
                ActualStartDateTimeUtc = model.ActualStartDateTimeUtc,
                EstimatedEndDateTimeUtc = model.EstimatedEndDateTimeUtc,
                InstanceParameters = model.InstanceParameters,
                JobParameters = model.JobParameters,
                Pid = model.Pid,
                PlannedStartDateTimeUtc = model.PlannedStartDateTimeUtc,
                Progress = model.Progress,
                State = model.State,
                JobId = model.Job.Id,
                TriggerId = model.Trigger.Id,
                Deleted = model.Deleted,
            };

            return entity;
        }
    }
}