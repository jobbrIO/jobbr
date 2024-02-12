using System;

namespace Jobbr.ComponentModel.Management.Model
{
    public class JobRun
    {
        public long Id { get; set; }
        public long JobId { get; set; }
        public string JobName { get; set; }
        public string JobType { get; set; }
        public long TriggerId { get; set; }
        public string TriggerType { get; set; }
        public string Definition { get; set; }
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public string Comment { get; set; }
        public JobRunStates State { get; set; }
        public double? Progress { get; set; }

        public DateTime PlannedStartDateTimeUtc { get; set; }
        public DateTime? ActualStartDateTimeUtc { get; set; }
        public DateTime? ActualEndDateTimeUtc { get; set; }
        public DateTime? EstimatedEndDateTimeUtc { get; set; }

        public string JobParameters { get; set; }
        public string InstanceParameters { get; set; }

        public bool Deleted { get; set; }
    }
}
