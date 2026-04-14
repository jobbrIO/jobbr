using System;
using Jobbr.ComponentModel.JobStorage.Model;
using Jobbr.Server.Core.Models;
using Jobbr.Server.Storage;

namespace Jobbr.Server.Core
{
    /// <summary>
    /// Service for jobs.
    /// </summary>
    public class JobService : IJobService
    {
        private readonly IJobbrRepository _repository;
        private static readonly Models.CoreMapper _mapper = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="JobService"/> class.
        /// </summary>
        /// <param name="repository">The Jobbr repository.</param>
        public JobService(IJobbrRepository repository)
        {
            _repository = repository;
        }

        /// <inheritdoc/>
        public JobModel Add(JobModel model)
        {
            var entity = _mapper.ForgeJob(model);

            _repository.AddJob(entity);
            model.Id = entity.Id;

            return model;
        }

        /// <inheritdoc/>
        public void Update(JobModel model)
        {
            var entity = _mapper.ForgeJob(model);

            var fromDb = _repository.GetJob(model.Id);

            fromDb.Parameters = entity.Parameters;
            fromDb.Title = entity.Title;
            fromDb.UpdatedDateTimeUtc = DateTime.UtcNow;

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Delete(long id)
        {
            var job = _repository.GetJob(id);
            job.Deleted = true;
        }
    }
}