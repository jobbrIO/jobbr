using System;

namespace Jobbr.ComponentModel.JobStorage.Model
{
    [Serializable]
    public class JobRun
    {
        public long Id { get; set; }
        public Job Job { get; set; }
        public JobTriggerBase Trigger { get; set; }
        public JobRunStates State { get; set; }
        public double? Progress { get; set; }

        public DateTime PlannedStartDateTimeUtc { get; set; }
        public DateTime? ActualStartDateTimeUtc { get; set; }
        public DateTime? ActualEndDateTimeUtc { get; set; }
        public DateTime? EstimatedEndDateTimeUtc { get; set; }

        public string JobParameters { get; set; }		
        public string InstanceParameters { get; set; }		

        public int? Pid { get; set; }
        public bool Deleted { get; set; }
    }
}
