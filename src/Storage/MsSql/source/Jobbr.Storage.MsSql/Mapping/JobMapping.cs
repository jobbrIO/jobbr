using Jobbr.ComponentModel.JobStorage.Model;

namespace Jobbr.Storage.MsSql.Mapping
{
    public static class JobMapping
    {
        public static Job ToModel(this Entities.Job entity)
        {
            var job = new Job
            {
                Id = entity.Id,
                CreatedDateTimeUtc = entity.CreatedDateTimeUtc,
                Parameters = entity.Parameters,
                Title = entity.Title,
                Type = entity.Type,
                UniqueName = entity.UniqueName,
                UpdatedDateTimeUtc = entity.UpdatedDateTimeUtc,
                Deleted = entity.Deleted,
                MaxConcurrentJobRuns = entity.MaxConcurrentJobRuns,
            };

            return job;
        }

        public static Entities.Job ToEntity(this Job model)
        {
            var entity = new Entities.Job
            {
                Id = model.Id,
                CreatedDateTimeUtc = model.CreatedDateTimeUtc,
                Parameters = model.Parameters,
                Title = model.Title,
                Type = model.Type,
                UniqueName = model.UniqueName,
                UpdatedDateTimeUtc = model.UpdatedDateTimeUtc,
                Deleted = model.Deleted,
                MaxConcurrentJobRuns = model.MaxConcurrentJobRuns,
            };

            return entity;
        }
    }
}
