using System;
using LiteDB;

namespace Jobbr.Storage.LiteDB.Entities
{
    public class JobEntity
    {
        [BsonId]
        public long Id { get; set; }

        public string UniqueName { get; set; }

        public string Title { get; set; }

        public string Parameters { get; set; }

        public string Type { get; set; }

        public DateTime? UpdatedDateTimeUtc { get; set; }

        public DateTime? CreatedDateTimeUtc { get; set; }

        public bool Deleted { get; set; }

        public int MaxConcurrentJobRuns { get; set; }
    }
}