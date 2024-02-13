using System;

namespace Jobbr.ComponentModel.JobStorage.Model
{
    [Serializable]
    public class RecurringTrigger : JobTriggerBase
    {
        public DateTime? StartDateTimeUtc { get; set; }

        public DateTime? EndDateTimeUtc { get; set; }

        public string Definition { get; set; }

        public bool NoParallelExecution { get; set; }
    }
}
