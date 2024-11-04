using System;

namespace Jobbr.ComponentModel.Management.Model
{
    public class RecurringTrigger : IJobTrigger
    {
        public DateTime? StartDateTimeUtc { get; set; }

        public DateTime? EndDateTimeUtc { get; set; }

        public string Definition { get; set; }
        public long Id { get; set; }
        public long JobId { get; set; }
        public string Comment { get; set; }
        public bool IsActive { get; set; }
        public string Parameters { get; set; }
        public string UserDisplayName { get; set; }
        public string UserId { get; set; }
        public bool NoParallelExecution { get; set; }
        public bool Deleted { get; set; }
    }
}
