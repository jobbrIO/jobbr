using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Jobbr.ComponentModel.JobStorage;
using Jobbr.ComponentModel.JobStorage.Model;
using Jobbr.Storage.LiteDB.Entities;
using Jobbr.Storage.LiteDB.Mapping;
using LiteDB;

namespace Jobbr.Storage.LiteDB
{
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access", Justification = "Keep the private implementation close to the public function")]
    public class LiteDbStorageProvider : IJobStorageProvider, IDisposable
    {
        private readonly LiteDatabase _database;
        private readonly ILiteCollection<JobEntity> _jobsCollection;
        private readonly ILiteCollection<TriggerEntity> _triggersCollection;
        private readonly ILiteCollection<JobRunEntity> _jobRunsCollection;
        private readonly object _idGenerationLock = new object();
        private bool _disposed = false;

        public LiteDbStorageProvider(JobbrLiteDbConfiguration configuration)
        {
            _database = new LiteDatabase(configuration.ConnectionString);

            _jobsCollection = _database.GetCollection<JobEntity>("jobs");
            _triggersCollection = _database.GetCollection<TriggerEntity>("triggers");
            _jobRunsCollection = _database.GetCollection<JobRunEntity>("jobruns");

            EnsureIndexes();
        }

        private void EnsureIndexes()
        {
            // Create indexes for better performance
            _jobsCollection.EnsureIndex(x => x.UniqueName, unique: true);
            _jobsCollection.EnsureIndex(x => x.Type);
            _jobsCollection.EnsureIndex(x => x.Deleted);

            _triggersCollection.EnsureIndex(x => x.JobId);
            _triggersCollection.EnsureIndex(x => x.IsActive);
            _triggersCollection.EnsureIndex(x => x.Deleted);
            _triggersCollection.EnsureIndex(x => new { JobId = x.JobId, Id = x.Id }, unique: true);

            _jobRunsCollection.EnsureIndex(x => x.JobId);
            _jobRunsCollection.EnsureIndex(x => x.TriggerId);
            _jobRunsCollection.EnsureIndex(x => x.State);
            _jobRunsCollection.EnsureIndex(x => x.Deleted);
            _jobRunsCollection.EnsureIndex(x => x.PlannedStartDateTimeUtc);
        }

        #region Jobs

        public void AddJob(Job job)
        {
            var entity = job.ToEntity();
            entity.CreatedDateTimeUtc = DateTime.UtcNow;
            entity.UpdatedDateTimeUtc = DateTime.UtcNow;

            if (entity.Id == 0)
            {
                entity.Id = GenerateId("jobs");
            }

            _jobsCollection.Insert(entity);
            job.Id = entity.Id;
            job.CreatedDateTimeUtc = entity.CreatedDateTimeUtc;
            job.UpdatedDateTimeUtc = entity.UpdatedDateTimeUtc;
        }

        public void DeleteJob(long jobId)
        {
            var job = _jobsCollection.FindById(jobId);
            if (job != null)
            {
                job.Deleted = true;
                job.UpdatedDateTimeUtc = DateTime.UtcNow;
                _jobsCollection.Update(job);
            }
        }

        public long GetJobsCount()
        {
            return _jobsCollection.Count(x => !x.Deleted);
        }

        public PagedResult<Job> GetJobs(int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, string query = null, bool showDeleted = false, params string[] sort)
        {
            var queryable = _jobsCollection.Query();

            if (!showDeleted)
            {
                queryable = queryable.Where(x => !x.Deleted);
            }

            if (!string.IsNullOrEmpty(jobTypeFilter))
            {
                queryable = queryable.Where(x => x.Type.Contains(jobTypeFilter));
            }

            if (!string.IsNullOrEmpty(jobUniqueNameFilter))
            {
                queryable = queryable.Where(x => x.UniqueName.Contains(jobUniqueNameFilter));
            }

            if (!string.IsNullOrEmpty(query))
            {
                queryable = queryable.Where(x => x.Title.Contains(query) || x.UniqueName.Contains(query) || x.Type.Contains(query));
            }

            var totalItems = queryable.Count();

            var items = queryable
                .ToEnumerable()
                .ApplyOrdering(sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.ToModel())
                .ToList();

            return new PagedResult<Job>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = items
            };
        }

        public Job GetJobById(long id)
        {
            var entity = _jobsCollection.FindById(id);
            return entity?.ToModel();
        }

        public Job GetJobByUniqueName(string identifier)
        {
            var entity = _jobsCollection.FindOne(x => x.UniqueName == identifier);
            return entity?.ToModel();
        }

        public void Update(Job job)
        {
            var entity = job.ToEntity();
            entity.UpdatedDateTimeUtc = DateTime.UtcNow;
            _jobsCollection.Update(entity);
            job.UpdatedDateTimeUtc = entity.UpdatedDateTimeUtc;
        }

        #endregion

        #region Triggers

        public void AddTrigger(long jobId, RecurringTrigger trigger)
        {
            var entity = trigger.ToEntity();
            entity.JobId = jobId;
            entity.CreatedDateTimeUtc = DateTime.UtcNow;

            if (entity.Id == 0)
            {
                entity.Id = GenerateTriggerId(jobId);
            }

            _triggersCollection.Insert(entity);
            trigger.Id = entity.Id;
            trigger.JobId = jobId;
            trigger.CreatedDateTimeUtc = entity.CreatedDateTimeUtc;
        }

        public void AddTrigger(long jobId, InstantTrigger trigger)
        {
            var entity = trigger.ToEntity();
            entity.JobId = jobId;
            entity.CreatedDateTimeUtc = DateTime.UtcNow;

            if (entity.Id == 0)
            {
                entity.Id = GenerateTriggerId(jobId);
            }

            _triggersCollection.Insert(entity);
            trigger.Id = entity.Id;
            trigger.JobId = jobId;
            trigger.CreatedDateTimeUtc = entity.CreatedDateTimeUtc;
        }

        public void AddTrigger(long jobId, ScheduledTrigger trigger)
        {
            var entity = trigger.ToEntity();
            entity.JobId = jobId;
            entity.CreatedDateTimeUtc = DateTime.UtcNow;

            if (entity.Id == 0)
            {
                entity.Id = GenerateTriggerId(jobId);
            }

            _triggersCollection.Insert(entity);
            trigger.Id = entity.Id;
            trigger.JobId = jobId;
            trigger.CreatedDateTimeUtc = entity.CreatedDateTimeUtc;
        }

        public JobTriggerBase GetTriggerById(long jobId, long triggerId)
        {
            var entity = _triggersCollection.FindOne(x => x.JobId == jobId && x.Id == triggerId);
            return entity?.ToModel();
        }

        public PagedResult<JobTriggerBase> GetTriggersByJobId(long jobId, int page = 1, int pageSize = 50, bool showDeleted = false)
        {
            var queryable = _triggersCollection.Query().Where(x => x.JobId == jobId);

            if (!showDeleted)
            {
                queryable = queryable.Where(x => !x.Deleted);
            }

            var totalItems = queryable.Count();

            var items = queryable
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToList()
                .Select(x => x.ToModel())
                .ToList();

            return new PagedResult<JobTriggerBase>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = items
            };
        }

        public PagedResult<JobTriggerBase> GetActiveTriggers(int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, string query = null, params string[] sort)
        {
            var queryable = _triggersCollection.Query().Where(x => x.IsActive && !x.Deleted);

            var totalItems = queryable.Count();

            var items = queryable
                .ToEnumerable()
                .ApplyOrdering(sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.ToModel())
                .ToList();

            // Filter by job properties if needed
            if (!string.IsNullOrEmpty(jobTypeFilter) || !string.IsNullOrEmpty(jobUniqueNameFilter))
            {
                var jobIds = new HashSet<long>();
                var jobQuery = _jobsCollection.Query().Where(x => !x.Deleted);

                if (!string.IsNullOrEmpty(jobTypeFilter))
                {
                    jobQuery = jobQuery.Where(x => x.Type.Contains(jobTypeFilter));
                }

                if (!string.IsNullOrEmpty(jobUniqueNameFilter))
                {
                    jobQuery = jobQuery.Where(x => x.UniqueName.Contains(jobUniqueNameFilter));
                }

                jobIds = jobQuery.ToList().Select(x => x.Id).ToHashSet();
                items = items.Where(x => jobIds.Contains(x.JobId)).ToList();
                totalItems = items.Count;
            }

            return new PagedResult<JobTriggerBase>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = items
            };
        }

        public void DisableTrigger(long jobId, long triggerId)
        {
            var trigger = _triggersCollection.FindOne(x => x.JobId == jobId && x.Id == triggerId);
            if (trigger != null)
            {
                trigger.IsActive = false;
                _triggersCollection.Update(trigger);
            }
        }

        public void EnableTrigger(long jobId, long triggerId)
        {
            var trigger = _triggersCollection.FindOne(x => x.JobId == jobId && x.Id == triggerId);
            if (trigger != null)
            {
                trigger.IsActive = true;
                _triggersCollection.Update(trigger);
            }
        }

        public void DeleteTrigger(long jobId, long triggerId)
        {
            var trigger = _triggersCollection.FindOne(x => x.JobId == jobId && x.Id == triggerId);
            if (trigger != null)
            {
                trigger.Deleted = true;
                _triggersCollection.Update(trigger);
            }
        }

        public void Update(long jobId, InstantTrigger trigger)
        {
            var entity = trigger.ToEntity();
            entity.JobId = jobId;
            _triggersCollection.Update(entity);
        }

        public void Update(long jobId, ScheduledTrigger trigger)
        {
            var entity = trigger.ToEntity();
            entity.JobId = jobId;
            _triggersCollection.Update(entity);
        }

        public void Update(long jobId, RecurringTrigger trigger)
        {
            var entity = trigger.ToEntity();
            entity.JobId = jobId;
            _triggersCollection.Update(entity);
        }

        #endregion

        #region JobRuns

        public void AddJobRun(JobRun jobRun)
        {
            var entity = jobRun.ToEntity();

            if (entity.Id == 0)
            {
                entity.Id = GenerateId("jobruns");
            }

            _jobRunsCollection.Insert(entity);
            jobRun.Id = entity.Id;
        }

        public JobRun GetJobRunById(long id)
        {
            var entity = _jobRunsCollection.FindById(id);
            if (entity == null)
            {
                return null;
            }

            var job = _jobsCollection.FindById(entity.JobId)?.ToModel();
            var trigger = _triggersCollection.FindOne(x => x.JobId == entity.JobId && x.Id == entity.TriggerId)?.ToModel();

            return entity.ToModel(job, trigger);
        }

        public JobRun GetLastJobRunByTriggerId(long jobId, long triggerId, DateTime utcNow)
        {
            var entity = _jobRunsCollection.Query()
                .Where(x => x.JobId == jobId && x.TriggerId == triggerId && x.PlannedStartDateTimeUtc <= utcNow && !x.Deleted)
                .OrderByDescending(x => x.PlannedStartDateTimeUtc)
                .FirstOrDefault();

            if (entity == null)
            {
                return null;
            }

            var job = _jobsCollection.FindById(entity.JobId)?.ToModel();
            var trigger = _triggersCollection.FindOne(x => x.JobId == entity.JobId && x.Id == entity.TriggerId)?.ToModel();

            return entity.ToModel(job, trigger);
        }

        public JobRun GetNextJobRunByTriggerId(long jobId, long triggerId, DateTime utcNow)
        {
            var entity = _jobRunsCollection.Query()
                .Where(x => x.JobId == jobId && x.TriggerId == triggerId && x.PlannedStartDateTimeUtc > utcNow && !x.Deleted)
                .OrderBy(x => x.PlannedStartDateTimeUtc)
                .FirstOrDefault();

            if (entity == null)
            {
                return null;
            }

            var job = _jobsCollection.FindById(entity.JobId)?.ToModel();
            var trigger = _triggersCollection.FindOne(x => x.JobId == entity.JobId && x.Id == entity.TriggerId)?.ToModel();

            return entity.ToModel(job, trigger);
        }

        public PagedResult<JobRun> GetJobRuns(int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, string query = null, bool showDeleted = false, params string[] sort)
        {
            var queryable = _jobRunsCollection.Query();

            if (!showDeleted)
            {
                queryable = queryable.Where(x => !x.Deleted);
            }

            var totalItems = queryable.Count();

            var entities = queryable
                .ToEnumerable()
                .ApplyOrdering(sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = new List<JobRun>();
            foreach (var entity in entities)
            {
                var job = _jobsCollection.FindById(entity.JobId)?.ToModel();
                var trigger = _triggersCollection.FindOne(x => x.JobId == entity.JobId && x.Id == entity.TriggerId)?.ToModel();

                // Apply filters if specified
                if (!string.IsNullOrEmpty(jobTypeFilter) && (job?.Type == null || !job.Type.Contains(jobTypeFilter)))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(jobUniqueNameFilter) && (job?.UniqueName == null || !job.UniqueName.Contains(jobUniqueNameFilter)))
                {
                    continue;
                }

                items.Add(entity.ToModel(job, trigger));
            }

            return new PagedResult<JobRun>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = items
            };
        }

        public PagedResult<JobRun> GetJobRunsByJobId(int jobId, int page = 1, int pageSize = 50, bool showDeleted = false, params string[] sort)
        {
            var queryable = _jobRunsCollection.Query().Where(x => x.JobId == jobId);

            if (!showDeleted)
            {
                queryable = queryable.Where(x => !x.Deleted);
            }

            var totalItems = queryable.Count();

            var entities = queryable
                .ToEnumerable()
                .ApplyOrdering(sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = new List<JobRun>();
            var job = _jobsCollection.FindById(jobId)?.ToModel();

            foreach (var entity in entities)
            {
                var trigger = _triggersCollection.FindOne(x => x.JobId == entity.JobId && x.Id == entity.TriggerId)?.ToModel();
                items.Add(entity.ToModel(job, trigger));
            }

            return new PagedResult<JobRun>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = items
            };
        }

        public PagedResult<JobRun> GetJobRunsByUserId(string userId, int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, bool showDeleted = false, params string[] sort)
        {
            // Get trigger IDs for the user
            var triggerEntities = _triggersCollection.Query()
                .Where(x => x.UserId == userId)
                .ToList();

            var triggerLookup = triggerEntities.ToDictionary(t => new { t.JobId, t.Id }, t => t.ToModel());

            var queryable = _jobRunsCollection.Query();

            if (!showDeleted)
            {
                queryable = queryable.Where(x => !x.Deleted);
            }

            var entities = queryable.ToList()
                .Where(jr => triggerLookup.ContainsKey(new { JobId = jr.JobId, Id = jr.TriggerId }))
                .AsEnumerable()
                .ApplyOrdering(sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = new List<JobRun>();
            foreach (var entity in entities)
            {
                var job = _jobsCollection.FindById(entity.JobId)?.ToModel();
                var trigger = triggerLookup.GetValueOrDefault(new { JobId = entity.JobId, Id = entity.TriggerId });

                // Apply filters if specified
                if (!string.IsNullOrEmpty(jobTypeFilter) && (job?.Type == null || !job.Type.Contains(jobTypeFilter)))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(jobUniqueNameFilter) && (job?.UniqueName == null || !job.UniqueName.Contains(jobUniqueNameFilter)))
                {
                    continue;
                }

                items.Add(entity.ToModel(job, trigger));
            }

            return new PagedResult<JobRun>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = items.Count,
                Items = items
            };
        }

        public PagedResult<JobRun> GetJobRunsByTriggerId(long jobId, long triggerId, int page = 1, int pageSize = 50, bool showDeleted = false, params string[] sort)
        {
            var queryable = _jobRunsCollection.Query().Where(x => x.JobId == jobId && x.TriggerId == triggerId);

            if (!showDeleted)
            {
                queryable = queryable.Where(x => !x.Deleted);
            }

            var totalItems = queryable.Count();

            var entities = queryable
                .ToEnumerable()
                .ApplyOrdering(sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = new List<JobRun>();
            var job = _jobsCollection.FindById(jobId)?.ToModel();
            var trigger = _triggersCollection.FindOne(x => x.JobId == jobId && x.Id == triggerId)?.ToModel();

            foreach (var entity in entities)
            {
                items.Add(entity.ToModel(job, trigger));
            }

            return new PagedResult<JobRun>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = items
            };
        }

        public PagedResult<JobRun> GetJobRunsByUserDisplayName(string userDisplayName, int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, bool showDeleted = false, params string[] sort)
        {
            // Get trigger IDs for the user display name
            var triggerEntities = _triggersCollection.Query()
                .Where(x => x.UserDisplayName == userDisplayName)
                .ToList();

            var triggerLookup = triggerEntities.ToDictionary(t => new { t.JobId, t.Id }, t => t.ToModel());

            var queryable = _jobRunsCollection.Query();

            if (!showDeleted)
            {
                queryable = queryable.Where(x => !x.Deleted);
            }

            var entities = queryable.ToList()
                .Where(jr => triggerLookup.ContainsKey(new { JobId = jr.JobId, Id = jr.TriggerId }))
                .AsEnumerable()
                .ApplyOrdering(sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = new List<JobRun>();
            foreach (var entity in entities)
            {
                var job = _jobsCollection.FindById(entity.JobId)?.ToModel();
                var trigger = triggerLookup.GetValueOrDefault(new { JobId = entity.JobId, Id = entity.TriggerId });

                // Apply filters if specified
                if (!string.IsNullOrEmpty(jobTypeFilter) && (job?.Type == null || !job.Type.Contains(jobTypeFilter)))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(jobUniqueNameFilter) && (job?.UniqueName == null || !job.UniqueName.Contains(jobUniqueNameFilter)))
                {
                    continue;
                }

                items.Add(entity.ToModel(job, trigger));
            }

            return new PagedResult<JobRun>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = items.Count,
                Items = items
            };
        }

        public PagedResult<JobRun> GetJobRunsByState(JobRunStates state, int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, string query = null, bool showDeleted = false, params string[] sort)
        {
            var queryable = _jobRunsCollection.Query().Where(x => x.State == state);

            if (!showDeleted)
            {
                queryable = queryable.Where(x => !x.Deleted);
            }

            var totalItems = queryable.Count();

            var entities = queryable
                .ToEnumerable()
                .ApplyOrdering(sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = new List<JobRun>();
            foreach (var entity in entities)
            {
                var job = _jobsCollection.FindById(entity.JobId)?.ToModel();
                var trigger = _triggersCollection.FindOne(x => x.JobId == entity.JobId && x.Id == entity.TriggerId)?.ToModel();

                // Apply filters if specified
                if (!string.IsNullOrEmpty(jobTypeFilter) && (job?.Type == null || !job.Type.Contains(jobTypeFilter)))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(jobUniqueNameFilter) && (job?.UniqueName == null || !job.UniqueName.Contains(jobUniqueNameFilter)))
                {
                    continue;
                }

                items.Add(entity.ToModel(job, trigger));
            }

            return new PagedResult<JobRun>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = items
            };
        }

        public PagedResult<JobRun> GetJobRunsByStates(JobRunStates[] states, int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, string query = null, bool showDeleted = false, params string[] sort)
        {
            var stateSet = states.ToHashSet();
            var queryable = _jobRunsCollection.Query().Where(x => stateSet.Contains(x.State));

            if (!showDeleted)
            {
                queryable = queryable.Where(x => !x.Deleted);
            }

            var totalItems = queryable.Count();

            var entities = queryable
                .ToEnumerable()
                .ApplyOrdering(sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = new List<JobRun>();
            foreach (var entity in entities)
            {
                var job = _jobsCollection.FindById(entity.JobId)?.ToModel();
                var trigger = _triggersCollection.FindOne(x => x.JobId == entity.JobId && x.Id == entity.TriggerId)?.ToModel();

                // Apply filters if specified
                if (!string.IsNullOrEmpty(jobTypeFilter) && (job?.Type == null || !job.Type.Contains(jobTypeFilter)))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(jobUniqueNameFilter) && (job?.UniqueName == null || !job.UniqueName.Contains(jobUniqueNameFilter)))
                {
                    continue;
                }

                items.Add(entity.ToModel(job, trigger));
            }

            return new PagedResult<JobRun>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = items
            };
        }

        public void Update(JobRun jobRun)
        {
            var entity = jobRun.ToEntity();
            _jobRunsCollection.Update(entity);
        }

        public void UpdateProgress(long jobRunId, double? progress)
        {
            var entity = _jobRunsCollection.FindById(jobRunId);
            if (entity != null)
            {
                entity.Progress = progress;
                _jobRunsCollection.Update(entity);
            }
        }

        #endregion

        public bool IsAvailable()
        {
            try
            {
                return _database != null && _jobsCollection != null;
            }
            catch
            {
                return false;
            }
        }

        #region Private Methods

        private long GenerateId(string collectionName)
        {
            lock (_idGenerationLock)
            {
                var counterCollection = _database.GetCollection<BsonDocument>("counters");
                var counter = counterCollection.FindById(collectionName);

                if (counter == null)
                {
                    counter = new BsonDocument { ["_id"] = collectionName, ["value"] = 1L };
                    counterCollection.Insert(counter);
                    return 1L;
                }

                var newValue = counter["value"].AsInt64 + 1;
                counter["value"] = newValue;
                counterCollection.Update(counter);
                return newValue;
            }
        }

        private long GenerateTriggerId(long jobId)
        {
            lock (_idGenerationLock)
            {
                // Find the maximum trigger ID for this job
                var maxId = _triggersCollection.Query()
                    .Where(x => x.JobId == jobId)
                    .Select(x => x.Id)
                    .ToList()
                    .DefaultIfEmpty(0)
                    .Max();

                return maxId + 1;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _database?.Dispose();
                _disposed = true;
            }
        }

        #endregion
    }
}