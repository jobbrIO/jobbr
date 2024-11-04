using System;

namespace Jobbr.ComponentModel.JobStorage.Model
{
    [Serializable]
    public class InstantTrigger : JobTriggerBase
    {
        public int DelayedMinutes { get; set; }
    }
}