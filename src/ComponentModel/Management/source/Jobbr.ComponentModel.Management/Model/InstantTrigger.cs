namespace Jobbr.ComponentModel.Management.Model
{
    public class InstantTrigger : IJobTrigger
    {
        public int DelayedMinutes { get; set; }
        public long Id { get; set; }
        public string Comment { get; set; }
        public bool IsActive { get; set; }
        public string Parameters { get; set; }
        public string UserDisplayName { get; set; }
        public string UserId { get; set; }
        public long JobId { get; set; }
        public bool Deleted { get; set; }
    }
}