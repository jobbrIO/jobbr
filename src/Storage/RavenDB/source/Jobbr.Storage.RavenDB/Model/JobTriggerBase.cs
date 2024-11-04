using System;

namespace Jobbr.Storage.RavenDB.Model
{
    public abstract class JobTriggerBase
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public string Parameters { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDateTimeUtc { get; set; }
    }
}