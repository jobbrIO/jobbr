using System;
using System.Collections.Generic;
using Raven.Imports.Newtonsoft.Json;

namespace Jobbr.Storage.RavenDB.Model
{
    public class Job
    {
        public const string CollectionPrefix = "Jobs";

        public Job()
        {
            InstantTriggers = new List<InstantTrigger>();
            ScheduledTriggers = new List<ScheduledTrigger>();
            RecurringTriggers = new List<RecurringTrigger>();
        }

        public string Id { get; set; }
        public string UniqueName { get; set; }
        public string Title { get; set; }
        public string Parameters { get; set; }
        public string Type { get; set; }
        public DateTime? UpdatedDateTimeUtc { get; set; }
        public DateTime? CreatedDateTimeUtc { get; set; }
        public long LastTriggerId { get; set; }

        public List<InstantTrigger> InstantTriggers { get; set; }
        public List<ScheduledTrigger> ScheduledTriggers { get; set; }
        public List<RecurringTrigger> RecurringTriggers { get; set; }

        [JsonIgnore]
        public int TriggerCount => InstantTriggers.Count + ScheduledTriggers.Count + RecurringTriggers.Count;
        
        [JsonIgnore]
        public IEnumerable<JobTriggerBase> AllTriggers
        {
            get
            {
                foreach (var instantTrigger in InstantTriggers)
                {
                    yield return instantTrigger;
                }

                foreach (var scheduledTrigger in ScheduledTriggers)
                {
                    yield return scheduledTrigger;
                }

                foreach (var recurringTrigger in RecurringTriggers)
                {
                    yield return recurringTrigger;
                }
            }
        }
    }
}
