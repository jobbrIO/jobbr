using System;
using ServiceStack.DataAnnotations;

namespace Jobbr.Storage.MsSql.Entities
{
    [Alias("Triggers")]
    public class Trigger
    {
        [AutoIncrement]
        public long Id { get; set; }

        public TriggerType Type { get; set; }

        [ForeignKey(typeof(Job))]
        public long JobId { get; set; }

        public bool IsActive { get; set; }

        [StringLength(100)]
        public string UserId { get; set; }

        [StringLength(100)]
        public string UserDisplayName { get; set; }

        [CustomField("NVARCHAR(MAX)")]
        public string Parameters { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedDateTimeUtc { get; set; }

        public DateTime? StartDateTimeUtc { get; set; }

        public DateTime? EndDateTimeUtc { get; set; }

        public string Definition { get; set; }

        public bool NoParallelExecution { get; set; }

        public int DelayedMinutes { get; set; }

        public bool Deleted { get; set; }
    }
}