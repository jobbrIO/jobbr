using System;

namespace Jobbr.ComponentModel.JobStorage.Model
{
    /// <summary>
    /// Base class for triggers. 
    /// Note: Composite key contains Id and JobId property.
    /// </summary>
    [Serializable]
    public class JobTriggerBase
    {
        public long Id { get; set; }
        public long JobId { get; set; }
        public bool IsActive { get; set; }
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public string Parameters { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDateTimeUtc { get; set; }
        public bool Deleted { get; set; }
    }
}
