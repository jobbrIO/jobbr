using System;
using Jobbr.ComponentModel.JobStorage.Model;
using Jobbr.Storage.LiteDB.Entities;

namespace Jobbr.Storage.LiteDB.Mapping
{
    public static class TriggerMapping
    {
        public static TriggerEntity ToEntity(this JobTriggerBase trigger)
        {
            var entity = new TriggerEntity
            {
                Id = trigger.Id,
                JobId = trigger.JobId,
                IsActive = trigger.IsActive,
                UserId = trigger.UserId,
                UserDisplayName = trigger.UserDisplayName,
                Parameters = trigger.Parameters,
                Comment = trigger.Comment,
                CreatedDateTimeUtc = trigger.CreatedDateTimeUtc,
                Deleted = trigger.Deleted
            };

            switch (trigger)
            {
                case RecurringTrigger recurringTrigger:
                    entity.TriggerType = "Recurring";
                    entity.StartDateTimeUtc = recurringTrigger.StartDateTimeUtc;
                    entity.EndDateTimeUtc = recurringTrigger.EndDateTimeUtc;
                    entity.Definition = recurringTrigger.Definition;
                    entity.NoParallelExecution = recurringTrigger.NoParallelExecution;
                    break;

                case InstantTrigger instantTrigger:
                    entity.TriggerType = "Instant";
                    entity.DelayedMinutes = instantTrigger.DelayedMinutes;
                    break;

                case ScheduledTrigger scheduledTrigger:
                    entity.TriggerType = "Scheduled";
                    entity.StartDateTimeUtc = scheduledTrigger.StartDateTimeUtc;
                    break;

                default:
                    throw new ArgumentException($"Unknown trigger type: {trigger.GetType()}");
            }

            return entity;
        }

        public static JobTriggerBase ToModel(this TriggerEntity entity)
        {
            JobTriggerBase trigger = entity.TriggerType switch
            {
                "Recurring" => new RecurringTrigger
                {
                    StartDateTimeUtc = entity.StartDateTimeUtc,
                    EndDateTimeUtc = entity.EndDateTimeUtc,
                    Definition = entity.Definition,
                    NoParallelExecution = entity.NoParallelExecution
                },
                "Instant" => new InstantTrigger
                {
                    DelayedMinutes = entity.DelayedMinutes
                },
                "Scheduled" => new ScheduledTrigger
                {
                    StartDateTimeUtc = entity.StartDateTimeUtc ?? DateTime.UtcNow
                },
                _ => throw new ArgumentException($"Unknown trigger type: {entity.TriggerType}")
            };

            trigger.Id = entity.Id;
            trigger.JobId = entity.JobId;
            trigger.IsActive = entity.IsActive;
            trigger.UserId = entity.UserId;
            trigger.UserDisplayName = entity.UserDisplayName;
            trigger.Parameters = entity.Parameters;
            trigger.Comment = entity.Comment;
            trigger.CreatedDateTimeUtc = entity.CreatedDateTimeUtc;
            trigger.Deleted = entity.Deleted;

            return trigger;
        }
    }
}