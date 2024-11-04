using System;
using Jobbr.ComponentModel.JobStorage.Model;

namespace Jobbr.Storage.MsSql.Mapping
{
    public static class TriggerMapping
    {
        public static JobTriggerBase ToModel(this Entities.Trigger entity)
        {
            switch (entity.Type)
            {
                case Entities.TriggerType.InstantTrigger:
                    return new InstantTrigger
                    {
                        Id = entity.Id,
                        CreatedDateTimeUtc = entity.CreatedDateTimeUtc,
                        Parameters = entity.Parameters,
                        Comment = entity.Comment,
                        DelayedMinutes = entity.DelayedMinutes,
                        IsActive = entity.IsActive,
                        JobId = entity.JobId,
                        UserDisplayName = entity.UserDisplayName,
                        UserId = entity.UserId,
                        Deleted = entity.Deleted,
                    };

                case Entities.TriggerType.RecurringTrigger:
                    return new RecurringTrigger
                    {
                        Id = entity.Id,
                        UserId = entity.UserId,
                        UserDisplayName = entity.UserDisplayName,
                        JobId = entity.JobId,
                        IsActive = entity.IsActive,
                        CreatedDateTimeUtc = entity.CreatedDateTimeUtc,
                        Parameters = entity.Parameters,
                        Comment = entity.Comment,
                        Definition = entity.Definition,
                        EndDateTimeUtc = entity.EndDateTimeUtc,
                        NoParallelExecution = entity.NoParallelExecution,
                        StartDateTimeUtc = entity.StartDateTimeUtc,
                        Deleted = entity.Deleted,
                    };

                case Entities.TriggerType.ScheduledTrigger:
                    return new ScheduledTrigger
                    {
                        Id = entity.Id,
                        UserId = entity.UserId,
                        UserDisplayName = entity.UserDisplayName,
                        JobId = entity.JobId,
                        IsActive = entity.IsActive,
                        CreatedDateTimeUtc = entity.CreatedDateTimeUtc,
                        Parameters = entity.Parameters,
                        Comment = entity.Comment,
                        StartDateTimeUtc = entity.StartDateTimeUtc.GetValueOrDefault(),
                        Deleted = entity.Deleted,
                    };

                default:
                    throw new NotSupportedException("TriggerType not supported: " + entity.Type);
            }
        }

        public static Entities.Trigger ToEntity(this InstantTrigger model)
        {
            return new Entities.Trigger
            {
                Id = model.Id,
                UserId = model.UserId,
                UserDisplayName = model.UserDisplayName,
                JobId = model.JobId,
                IsActive = model.IsActive,
                Comment = model.Comment,
                Parameters = model.Parameters,
                CreatedDateTimeUtc = model.CreatedDateTimeUtc,
                Type = Entities.TriggerType.InstantTrigger,
                DelayedMinutes = model.DelayedMinutes,
                NoParallelExecution = false,
                Deleted = model.Deleted,
            };
        }

        public static Entities.Trigger ToEntity(this ScheduledTrigger model)
        {
            return new Entities.Trigger
            {
                Id = model.Id,
                UserId = model.UserId,
                UserDisplayName = model.UserDisplayName,
                JobId = model.JobId,
                IsActive = model.IsActive,
                Comment = model.Comment,
                Parameters = model.Parameters,
                CreatedDateTimeUtc = model.CreatedDateTimeUtc,
                Type = Entities.TriggerType.ScheduledTrigger,
                NoParallelExecution = false,
                StartDateTimeUtc = model.StartDateTimeUtc,
                Deleted = model.Deleted,
            };
        }

        public static Entities.Trigger ToEntity(this RecurringTrigger model)
        {
            return new Entities.Trigger
            {
                Id = model.Id,
                UserId = model.UserId,
                UserDisplayName = model.UserDisplayName,
                JobId = model.JobId,
                IsActive = model.IsActive,
                Comment = model.Comment,
                Parameters = model.Parameters,
                CreatedDateTimeUtc = model.CreatedDateTimeUtc,
                Type = Entities.TriggerType.RecurringTrigger,
                NoParallelExecution = model.NoParallelExecution,
                StartDateTimeUtc = model.StartDateTimeUtc,
                EndDateTimeUtc = model.EndDateTimeUtc,
                Definition = model.Definition,
                Deleted = model.Deleted,
            };
        }
    }
}