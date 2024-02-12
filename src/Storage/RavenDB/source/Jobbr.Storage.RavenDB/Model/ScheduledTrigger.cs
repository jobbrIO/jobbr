using System;

namespace Jobbr.Storage.RavenDB.Model
{
    public class ScheduledTrigger : JobTriggerBase
    {
        public DateTime StartDateTimeUtc { get; set; }
    }
}