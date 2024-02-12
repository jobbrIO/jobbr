using System;
using System.Collections.Generic;
using System.Linq;
using Jobbr.ComponentModel.JobStorage;
using Jobbr.ComponentModel.JobStorage.Model;
using Jobbr.Storage.RavenDB.Mapping;
using Jobbr.Storage.RavenDB.Model.Index;
using Raven.Client;
using Raven.Client.Indexes;
using Raven.Client.Linq;

namespace Jobbr.Storage.RavenDB
{
    public class RavenDbStorageProvider : IJobStorageProvider
    {
        private readonly IDocumentStore _documentStore;

        public RavenDbStorageProvider(IDocumentStore store)
        {
            _documentStore = store;
            IndexCreation.CreateIndexes(typeof(RavenDbStorageProvider).Assembly, this._documentStore);
        }

        public List<Job> GetJobs(int page = 0, int pageSize = 50)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var jobs = session.Query<Model.Job>().Skip(page * pageSize).Take(pageSize).ToList();

                return jobs.Select(s => s.ToModel()).ToList();
            }
        }

        public void AddJob(Job job)
        {
            var entity = job.ToEntity();

            using (var session = this._documentStore.OpenSession())
            {
                session.Store(entity);
                session.SaveChanges();
                job.Id = entity.Id.ParseId();
            }
        }

        public List<JobTriggerBase> GetTriggersByJobId(long jobId)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var job = session.Load<Model.Job>(jobId);

                var triggers = new List<JobTriggerBase>(job.TriggerCount);

                triggers.AddRange(job.InstantTriggers.Select(trigger => trigger.ToModel(jobId)));
                triggers.AddRange(job.ScheduledTriggers.Select(trigger => trigger.ToModel(jobId)));
                triggers.AddRange(job.RecurringTriggers.Select(trigger => trigger.ToModel(jobId)));

                return triggers;
            }
        }

        public void AddTrigger(long jobId, RecurringTrigger trigger)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var job = session.Load<Model.Job>(jobId);
                var entity = trigger.ToEntity();
                entity.Id = ++job.LastTriggerId;
                entity.CreatedDateTimeUtc = DateTime.UtcNow;

                job.RecurringTriggers.Add(entity);

                session.Store(job);
                session.SaveChanges();

                trigger.Id = entity.Id;
                trigger.CreatedDateTimeUtc = entity.CreatedDateTimeUtc;
            }
        }

        public void AddTrigger(long jobId, InstantTrigger trigger)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var job = session.Load<Model.Job>(jobId);
                var entity = trigger.ToEntity();
                entity.Id = ++job.LastTriggerId;
                entity.CreatedDateTimeUtc = DateTime.UtcNow;

                job.InstantTriggers.Add(entity);

                session.Store(job);
                session.SaveChanges();

                trigger.Id = entity.Id;
                trigger.CreatedDateTimeUtc = entity.CreatedDateTimeUtc;
            }
        }

        public void AddTrigger(long jobId, ScheduledTrigger trigger)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var job = session.Load<Model.Job>(jobId);
                var entity = trigger.ToEntity();
                entity.Id = ++job.LastTriggerId;
                entity.CreatedDateTimeUtc = DateTime.UtcNow;

                job.ScheduledTriggers.Add(entity);

                session.Store(job);
                session.SaveChanges();

                trigger.Id = entity.Id;
                trigger.CreatedDateTimeUtc = entity.CreatedDateTimeUtc;
            }
        }

        private void SetTriggerStatus(long jobId, long triggerId, bool isActive)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var job = session.Load<Model.Job>(jobId);
              
                var trigger = job.AllTriggers.First(p => p.Id == triggerId);
                trigger.IsActive = isActive;

                session.Store(job);
                session.SaveChanges();
            }
        }

        public void DisableTrigger(long jobId, long triggerId)
        {
            SetTriggerStatus(jobId, triggerId, false);
        }

        public void EnableTrigger(long jobId, long triggerId)
        {
            SetTriggerStatus(jobId, triggerId, true);
        }

        public List<JobTriggerBase> GetActiveTriggers()
        {
            using (var session = this._documentStore.OpenSession())
            {
                var jobIds = session.Query<TriggerIndex.Result, TriggerIndex>().Where(p => p.IsActive).Select(s => s.JobId).ToList();
                var jobs = session.Load<Model.Job>(jobIds);

                var activeTriggers = new List<JobTriggerBase>();

                foreach (var job in jobs)
                {
                    activeTriggers.AddRange(job.InstantTriggers.Where(p => p.IsActive).Select(trigger => trigger.ToModel(job.Id.ParseId())));
                    activeTriggers.AddRange(job.ScheduledTriggers.Where(p => p.IsActive).Select(trigger => trigger.ToModel(job.Id.ParseId())));
                    activeTriggers.AddRange(job.RecurringTriggers.Where(p => p.IsActive).Select(trigger => trigger.ToModel(job.Id.ParseId())));
                }

                return activeTriggers;
            }
        }

        public JobTriggerBase GetTriggerById(long jobId, long triggerId)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var job = session.Load<Model.Job>(jobId);
                var trigger = job.AllTriggers.First(p => p.Id == triggerId);

                var instantTrigger = trigger as Model.InstantTrigger;
                if (instantTrigger != null)
                {
                    return instantTrigger.ToModel(jobId);
                }

                var scheduledTrigger = trigger as Model.ScheduledTrigger;
                if (scheduledTrigger != null)
                {
                    return scheduledTrigger.ToModel(jobId);
                }

                var recurringTrigger = trigger as Model.RecurringTrigger;

                return recurringTrigger?.ToModel(jobId);
            }
        }

        public JobRun GetLastJobRunByTriggerId(long jobId, long triggerId, DateTime now)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var jobIdAsString = jobId.ToRavenId(Model.Job.CollectionPrefix);
                var lastJobRun = session.Query<JobRunIndex.Result, JobRunIndex>()
                                        .Where(p => p.JobId == jobIdAsString && 
                                                    p.TriggerId == triggerId && 
                                                    p.ActualStartDateTimeUtc.Value < now)
                                        .OrderByDescending(o => o.ActualStartDateTimeUtc)
                                        .ProjectFromIndexFieldsInto<Model.JobRun>()
                                        .FirstOrDefault();

                return lastJobRun?.ToModel();
            }
        }

        public JobRun GetNextJobRunByTriggerId(long jobId, long triggerId, DateTime now)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var jobIdAsString = jobId.ToRavenId(Model.Job.CollectionPrefix);
                var lastJobRun = session.Query<JobRunIndex.Result, JobRunIndex>()
                    .Where(p => p.JobId == jobIdAsString && 
                                p.TriggerId == triggerId && 
                                p.ActualEndDateTimeUtc >= now && 
                                p.State == Model.JobRunStates.Scheduled)
                    .OrderBy(o => o.PlannedStartDateTimeUtc)
                    .ProjectFromIndexFieldsInto<Model.JobRun>()
                    .FirstOrDefault();

                return lastJobRun?.ToModel();
            }
        }

        public void AddJobRun(JobRun jobRun)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var job = session.Load<Model.Job>(jobRun.JobId);
                var entity = jobRun.ToEntity(job);
                var trigger = job.AllTriggers.First(p => p.Id == jobRun.TriggerId);

                // denormalize data
                entity.UserId = trigger.UserId;
                entity.UserDisplayName = trigger.UserDisplayName;
                entity.Job = job;
                entity.Trigger = trigger;

                session.Store(entity);
                session.SaveChanges();

                jobRun.Id = entity.Id.ParseId();
            }
        }

        public List<JobRun> GetJobRuns(int page = 0, int pageSize = 50)
        {
            using (var session = this._documentStore.OpenSession())
            {
                return session.GetAll<Model.JobRun>().Skip(page * pageSize).Take(pageSize).ToList().Select(s => s.ToModel()).ToList();
            }
        }

        public void UpdateProgress(long jobRunId, double? progress)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var jobRun = session.Load<Model.JobRun>(jobRunId);
                jobRun.Progress = progress;

                session.Store(jobRun);
                session.SaveChanges();
            }
        }

        public void Update(JobRun jobRun)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var entity = session.Load<Model.JobRun>(jobRun.Id);
                jobRun.ApplyTo(entity);
                session.Store(entity);
                session.SaveChanges();
            }
        }

        public Job GetJobById(long id)
        {
            using (var session = this._documentStore.OpenSession())
            {
                return session.Load<Model.Job>(id).ToModel();
            }
        }

        public Job GetJobByUniqueName(string identifier)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var jobEntity = session.Query<Model.Job>().FirstOrDefault(p => p.UniqueName == identifier);

                return jobEntity?.ToModel();
            }
        }

        public JobRun GetJobRunById(long id)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var jobRunEntity = session.Load<Model.JobRun>(id);

                return jobRunEntity?.ToModel();
            }
        }

        public List<JobRun> GetJobRunsByUserId(string userId, int page = 0, int pageSize = 50)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var results = session.Query<JobRunIndex.Result, JobRunIndex>().OrderByDescending(o => o.PlannedStartDateTimeUtc).Where(p => p.UserId == userId).Skip(page * pageSize).Take(pageSize).As<Model.JobRun>().ToList();

                return results.Select(s => s.ToModel()).ToList();
            }
        }

        public List<JobRun> GetJobRunsByUserDisplayName(string userDisplayName, int page = 0, int pageSize = 50)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var results = session.Query<JobRunIndex.Result, JobRunIndex>().OrderByDescending(o => o.PlannedStartDateTimeUtc).Where(p => p.UserDisplayName == userDisplayName).Skip(page * pageSize).Take(pageSize).As<Model.JobRun>().ToList();

                return results.Select(s => s.ToModel()).ToList();
            }
        }

        public void Update(Job job)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var entity = job.ToEntity();

                session.Store(entity);
                session.SaveChanges();
            }
        }

        public void Update(long jobId, InstantTrigger trigger)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var job = session.Load<Model.Job>(jobId);

                var triggerFromDb = job.InstantTriggers.First(p => p.Id == trigger.Id);
                job.InstantTriggers.Remove(triggerFromDb);

                var entity = trigger.ToEntity();

                job.InstantTriggers.Add(entity);

                session.Store(job);
                session.SaveChanges();
            }
        }

        public void Update(long jobId, ScheduledTrigger trigger)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var job = session.Load<Model.Job>(jobId);

                var triggerFromDb = job.ScheduledTriggers.First(p => p.Id == trigger.Id);
                job.ScheduledTriggers.Remove(triggerFromDb);

                var entity = trigger.ToEntity();

                job.ScheduledTriggers.Add(entity);

                session.Store(job);
                session.SaveChanges();
            }
        }

        public void Update(long jobId, RecurringTrigger trigger)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var job = session.Load<Model.Job>(jobId);

                var triggerFromDb = job.RecurringTriggers.First(p => p.Id == trigger.Id);
                job.RecurringTriggers.Remove(triggerFromDb);

                var entity = trigger.ToEntity();

                job.RecurringTriggers.Add(entity);

                session.Store(job);
                session.SaveChanges();
            }
        }

        public List<JobRun> GetJobRunsByTriggerId(long jobId, long triggerId, int page = 0, int pageSize = 50)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var jobIdAsString = jobId.ToRavenId(Model.Job.CollectionPrefix);

                return session.Query<JobRunIndex.Result, JobRunIndex>()
                    .Where(p => p.JobId == jobIdAsString && p.TriggerId == triggerId)
                    .OrderByDescending(o => o.PlannedStartDateTimeUtc)
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .As<Model.JobRun>()
                    .ToList()
                    .Select(s => s.ToModel())
                    .ToList();
            }
        }

        public List<JobRun> GetJobRunsByState(JobRunStates state, int page = 0, int pageSize = 50)
        {
            using (var session = this._documentStore.OpenSession())
            {
                var stateFromModel = (Model.JobRunStates)state;

                return session.Query<Model.JobRun>()
                    .Where(p => p.State == stateFromModel)
                    .OrderByDescending(o => o.PlannedStartDateTimeUtc)
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .Select(s => s.ToModel())
                    .ToList();
            }
        }

        public long GetJobsCount()
        {
            using (var session = this._documentStore.OpenSession())
            {
                return session.Query<Model.Job>().Count();
            }
        }

        public bool IsAvailable()
        {
            try
            {
                GetJobsCount();
                return true;
            }
            catch
            {
                // ignore
            }

            return false;
        }
    }
}
