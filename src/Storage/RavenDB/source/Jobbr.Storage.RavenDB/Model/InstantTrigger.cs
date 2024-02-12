namespace Jobbr.Storage.RavenDB.Model
{
    public class InstantTrigger : JobTriggerBase
    {
        public int DelayedMinutes { get; set; }
    }
}