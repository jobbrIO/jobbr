using System;
using LiteDB;

namespace Jobbr.Storage.LiteDB.Entities
{
    public class TriggerEntity
    {
        [BsonId]
        public long Id { get; set; }

        public long JobId { get; set; }

        public string TriggerType { get; set; }

        public bool IsActive { get; set; }

        public string UserId { get; set; }

        public string UserDisplayName { get; set; }

        public string Parameters { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedDateTimeUtc { get; set; }

        public bool Deleted { get; set; }

        // For RecurringTrigger
        public DateTime? StartDateTimeUtc { get; set; }

        public DateTime? EndDateTimeUtc { get; set; }

        public string Definition { get; set; }

        public bool NoParallelExecution { get; set; }

        // For InstantTrigger
        public int DelayedMinutes { get; set; }

        // For ScheduledTrigger - uses StartDateTimeUtc
    }
}