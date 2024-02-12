using Jobbr.Storage.RavenDB.Model;

namespace Jobbr.Storage.RavenDB.Mapping
{
    public static class Mapper
    {
        public static Job ToEntity(this ComponentModel.JobStorage.Model.Job src)
        {
            return new Job
            {
                Id = src.Id > 0 ? $"{Job.CollectionPrefix}/{src.Id}" : null,
                CreatedDateTimeUtc = src.CreatedDateTimeUtc,
                Parameters = src.Parameters,
                Title = src.Title,
                Type = src.Type,
                UniqueName = src.UniqueName,
                UpdatedDateTimeUtc = src.UpdatedDateTimeUtc
            };
        }

        public static ComponentModel.JobStorage.Model.Job ToModel(this Job src)
        {
            return new ComponentModel.JobStorage.Model.Job
            {
                Id = src.Id.ParseId(),
                Title = src.Title,
                UniqueName = src.UniqueName,
                CreatedDateTimeUtc = src.CreatedDateTimeUtc,
                UpdatedDateTimeUtc = src.UpdatedDateTimeUtc,
                Parameters = src.Parameters,
                Type = src.Type
            };
        }

        public static RecurringTrigger ToEntity(this ComponentModel.JobStorage.Model.RecurringTrigger src)
        {
            return new RecurringTrigger
            {
                Id = src.Id,
                CreatedDateTimeUtc = src.CreatedDateTimeUtc,
                Parameters = src.Parameters,
                Comment = src.Comment,
                Definition = src.Definition,
                EndDateTimeUtc = src.EndDateTimeUtc,
                IsActive = src.IsActive,
                NoParallelExecution = src.NoParallelExecution,
                StartDateTimeUtc = src.StartDateTimeUtc,
                UserDisplayName = src.UserDisplayName,
                UserId = src.UserId
            };
        }

        public static ComponentModel.JobStorage.Model.RecurringTrigger ToModel(this RecurringTrigger src, long jobId)
        {
            return new ComponentModel.JobStorage.Model.RecurringTrigger
            {
                CreatedDateTimeUtc = src.CreatedDateTimeUtc,
                Parameters = src.Parameters,
                Id = src.Id,
                IsActive = src.IsActive,
                Comment = src.Comment,
                UserId = src.UserId,
                UserDisplayName = src.UserDisplayName,
                StartDateTimeUtc = src.StartDateTimeUtc,
                EndDateTimeUtc = src.EndDateTimeUtc,
                NoParallelExecution = src.NoParallelExecution,
                Definition = src.Definition,
                JobId = jobId
            };
        }

        public static ScheduledTrigger ToEntity(this ComponentModel.JobStorage.Model.ScheduledTrigger src)
        {
            return new ScheduledTrigger
            {
                Id = src.Id,
                CreatedDateTimeUtc = src.CreatedDateTimeUtc,
                Parameters = src.Parameters,
                StartDateTimeUtc = src.StartDateTimeUtc,
                IsActive = src.IsActive,
                UserId = src.UserId,
                Comment = src.Comment,
                UserDisplayName = src.UserDisplayName
            };
        }

        public static ComponentModel.JobStorage.Model.ScheduledTrigger ToModel(this ScheduledTrigger src, long jobId)
        {
            return new ComponentModel.JobStorage.Model.ScheduledTrigger
            {
                Id = src.Id,
                CreatedDateTimeUtc = src.CreatedDateTimeUtc,
                Parameters = src.Parameters,
                StartDateTimeUtc = src.StartDateTimeUtc,
                IsActive = src.IsActive,
                UserId = src.UserId,
                Comment = src.Comment,
                UserDisplayName = src.UserDisplayName,
                JobId = jobId
            };
        }

        public static InstantTrigger ToEntity(this ComponentModel.JobStorage.Model.InstantTrigger src)
        {
            return new InstantTrigger
            {
                Id = src.Id,
                CreatedDateTimeUtc = src.CreatedDateTimeUtc,
                Parameters = src.Parameters,
                IsActive = src.IsActive,
                Comment = src.Comment,
                UserId = src.UserId,
                UserDisplayName = src.UserDisplayName,
                DelayedMinutes = src.DelayedMinutes
            };
        }

        public static ComponentModel.JobStorage.Model.InstantTrigger ToModel(this InstantTrigger src, long jobId)
        {
            return new ComponentModel.JobStorage.Model.InstantTrigger
            {
                Id = src.Id,
                CreatedDateTimeUtc = src.CreatedDateTimeUtc,
                Parameters = src.Parameters,
                IsActive = src.IsActive,
                Comment = src.Comment,
                UserId = src.UserId,
                UserDisplayName = src.UserDisplayName,
                DelayedMinutes = src.DelayedMinutes,
                JobId = jobId
            };
        }

        public static JobRun ToEntity(this ComponentModel.JobStorage.Model.JobRun src, Job job)
        {
            return new JobRun
            {
                Id = src.Id > 0 ? $"{JobRun.CollectionPrefix}/{src.Id}" : null,
                Job = job,
                ActualEndDateTimeUtc = src.ActualEndDateTimeUtc,
                ActualStartDateTimeUtc = src.ActualStartDateTimeUtc,
                EstimatedEndDateTimeUtc = src.EstimatedEndDateTimeUtc,
                InstanceParameters = src.InstanceParameters,
                JobParameters = src.JobParameters,
                Pid = src.Pid,
                PlannedStartDateTimeUtc = src.PlannedStartDateTimeUtc,
                Progress = src.Progress,
                State = (JobRunStates)src.State
            };
        }

        public static JobRun ApplyTo(this ComponentModel.JobStorage.Model.JobRun src, JobRun targetEntity)
        {
            targetEntity.Id = src.Id > 0 ? $"{JobRun.CollectionPrefix}/{src.Id}" : null;
            targetEntity.ActualEndDateTimeUtc = src.ActualEndDateTimeUtc;
            targetEntity.ActualStartDateTimeUtc = src.ActualStartDateTimeUtc;
            targetEntity.EstimatedEndDateTimeUtc = src.EstimatedEndDateTimeUtc;
            targetEntity.InstanceParameters = src.InstanceParameters;
            targetEntity.JobParameters = src.JobParameters;
            targetEntity.Pid = src.Pid;
            targetEntity.PlannedStartDateTimeUtc = src.PlannedStartDateTimeUtc;
            targetEntity.Progress = src.Progress;
            targetEntity.State = (JobRunStates)src.State;

            return targetEntity;
        }

        public static ComponentModel.JobStorage.Model.JobRun ToModel(this JobRun src)
        {
            return new ComponentModel.JobStorage.Model.JobRun
            {
                Id = src.Id.ParseId(),
                JobId = src.Job.Id.ParseId(),
                ActualEndDateTimeUtc = src.ActualEndDateTimeUtc,
                ActualStartDateTimeUtc = src.ActualStartDateTimeUtc,
                EstimatedEndDateTimeUtc = src.EstimatedEndDateTimeUtc,
                InstanceParameters = src.InstanceParameters,
                JobParameters = src.JobParameters,
                Pid = src.Pid,
                PlannedStartDateTimeUtc = src.PlannedStartDateTimeUtc,
                Progress = src.Progress,
                State = (ComponentModel.JobStorage.Model.JobRunStates)src.State,
                TriggerId = src.Trigger.Id
            };
        }
    }
}
