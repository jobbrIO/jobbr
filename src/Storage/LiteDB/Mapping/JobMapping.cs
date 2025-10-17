using Jobbr.ComponentModel.JobStorage.Model;
using Jobbr.Storage.LiteDB.Entities;

namespace Jobbr.Storage.LiteDB.Mapping
{
    public static class JobMapping
    {
        public static JobEntity ToEntity(this Job job)
        {
            return new JobEntity
            {
                Id = job.Id,
                UniqueName = job.UniqueName,
                Title = job.Title,
                Parameters = job.Parameters,
                Type = job.Type,
                UpdatedDateTimeUtc = job.UpdatedDateTimeUtc,
                CreatedDateTimeUtc = job.CreatedDateTimeUtc,
                Deleted = job.Deleted,
                MaxConcurrentJobRuns = job.MaxConcurrentJobRuns
            };
        }

        public static Job ToModel(this JobEntity entity)
        {
            return new Job
            {
                Id = entity.Id,
                UniqueName = entity.UniqueName,
                Title = entity.Title,
                Parameters = entity.Parameters,
                Type = entity.Type,
                UpdatedDateTimeUtc = entity.UpdatedDateTimeUtc,
                CreatedDateTimeUtc = entity.CreatedDateTimeUtc,
                Deleted = entity.Deleted,
                MaxConcurrentJobRuns = entity.MaxConcurrentJobRuns
            };
        }
    }
}