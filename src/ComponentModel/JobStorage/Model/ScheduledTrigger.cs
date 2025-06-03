using System;

namespace Jobbr.ComponentModel.JobStorage.Model
{
    public class ScheduledTrigger : JobTriggerBase
    {
        public DateTime StartDateTimeUtc { get; set; }
    }
}