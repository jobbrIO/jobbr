using System;
using Jobbr.ComponentModel.JobStorage.Model;
using ServiceStack.DataAnnotations;

namespace Jobbr.Storage.MsSql.Entities
{
    [Alias("JobRuns")]
    public class JobRun
    {
        [AutoIncrement]
        public long Id { get; set; }

        [ForeignKey(typeof(Job))]
        public long JobId { get; set; }

        [ForeignKey(typeof(Trigger))]
        public long TriggerId { get; set; }

        public JobRunStates State { get; set; }

        public double? Progress { get; set; }

        public DateTime PlannedStartDateTimeUtc { get; set; }

        public DateTime? ActualStartDateTimeUtc { get; set; }

        public DateTime? ActualEndDateTimeUtc { get; set; }

        public DateTime? EstimatedEndDateTimeUtc { get; set; }

        [CustomField("NVARCHAR(MAX)")]
        public string JobParameters { get; set; }

        [CustomField("NVARCHAR(MAX)")]
        public string InstanceParameters { get; set; }

        public int? Pid { get; set; }

        public bool Deleted { get; set; }
    }
}