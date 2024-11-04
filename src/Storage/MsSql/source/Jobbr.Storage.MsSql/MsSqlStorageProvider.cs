using System;
using System.Linq;
using Jobbr.ComponentModel.JobStorage;
using Jobbr.ComponentModel.JobStorage.Model;
using Jobbr.Storage.MsSql.Entities;
using Jobbr.Storage.MsSql.Mapping;
using ServiceStack.OrmLite;
using Job = Jobbr.ComponentModel.JobStorage.Model.Job;
using JobRun = Jobbr.ComponentModel.JobStorage.Model.JobRun;

namespace Jobbr.Storage.MsSql
{
    public class MsSqlStorageProvider : IJobStorageProvider
    {
        private readonly OrmLiteConnectionFactory _connectionFactory;
        private RetentionEnforcer _retentionEnforcer;

        public MsSqlStorageProvider(JobbrMsSqlConfiguration configuration)
        {
            _connectionFactory = new OrmLiteConnectionFactory(configuration.ConnectionString, configuration.DialectProvider);

            if (configuration.CreateTablesIfNotExists)
            {
                CreateTables();
            }

            if (configuration.Retention.HasValue)
            {
                _retentionEnforcer = new RetentionEnforcer(this, configuration.Retention.Value, configuration.RetentionEnforcementInterval);
            }
        }

        private void CreateTables()
        {
            using (var session = _connectionFactory.OpenDbConnection())
            {
                session.CreateTableIfNotExists<Entities.Job>();
                session.CreateTableIfNotExists<Trigger>();
                session.CreateTableIfNotExists<Entities.JobRun>();
            }
        }

        public void AddJob(Job job)
        {
            var entity = job.ToEntity();
            entity.CreatedDateTimeUtc = DateTime.UtcNow;

            using (var session = _connectionFactory.Open())
            {
                job.Id = session.Insert(entity, true);
            }
        }

        public void Update(Job job)
        {
            using (var connection = _connectionFactory.Open())
            {
                var entity = job.ToEntity();
                entity.UpdatedDateTimeUtc = DateTime.UtcNow;

                connection.Update(entity);
            }
        }

        public void DeleteJob(long jobId)
        {
            using (var connection = _connectionFactory.Open())
            {
                connection.Delete<Entities.Job>(job => job.Id == jobId);
            }
        }

        public Job GetJobById(long id)
        {
            using (var connection = _connectionFactory.Open())
            {
                return connection.SingleById<Entities.Job>(id)?.ToModel();
            }
        }

        public Job GetJobByUniqueName(string identifier)
        {
            using (var connection = _connectionFactory.Open())
            {
                var job = connection.Single<Entities.Job>(p => p.UniqueName == identifier);

                return job?.ToModel();
            }
        }

        public long GetJobsCount()
        {
            using (var connection = _connectionFactory.Open())
            {
                return connection.Count<Entities.Job>(p => p.Deleted == false);
            }
        }

        public PagedResult<Job> GetJobs(int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, string query = null, bool showDeleted = false, params string[] sort)
        {
            AssertOnlyOneFilterIsActive(jobTypeFilter, jobUniqueNameFilter, query);

            using (var connection = _connectionFactory.Open())
            {
                var sqlExpression = connection.From<Entities.Job>().Where(p => p.Deleted == showDeleted);

                if (jobTypeFilter != null)
                {
                    sqlExpression.Where(p => p.Type == jobTypeFilter);
                }
                else if (jobUniqueNameFilter != null)
                {
                    sqlExpression.Where(p => p.UniqueName == jobUniqueNameFilter);
                }
                else if (query != null)
                {
                    sqlExpression.Where(p => p.UniqueName.Contains(query) || p.Type.Contains(query) || p.Title.Contains(query));
                }

                var count = connection.Count(sqlExpression);

                sqlExpression.Ordered(sort);

                sqlExpression.Skip((page - 1) * pageSize).Take(pageSize);

                var rows = connection.Select(sqlExpression)
                    .ToList()
                    .Select(s => s.ToModel())
                    .ToList();

                return new PagedResult<Job>
                {
                    Items = rows,
                    PageSize = pageSize,
                    Page = page,
                    TotalItems = (int)count
                };
            }
        }

        public void AddJobRun(JobRun jobRun)
        {
            var entity = jobRun.ToEntity();

            using (var connection = _connectionFactory.Open())
            {
                jobRun.Id = connection.Insert(entity, true);
            }
        }

        public JobRun GetJobRunById(long id)
        {
            using (var connection = _connectionFactory.Open())
            {
                return connection.Select<JobRunInfo>(
                        connection.From<Entities.JobRun>()
                            .Join<Entities.JobRun, Trigger>((jr, t) => jr.TriggerId == t.Id)
                            .Join<Entities.JobRun, Entities.Job>((jr, j) => jr.JobId == j.Id)
                            .Where(p => p.Id == id))
                    .Select(s => s.ToModel()).FirstOrDefault();
            }
        }

        public void Update(JobRun jobRun)
        {
            using (var connection = _connectionFactory.Open())
            {
                var entity = jobRun.ToEntity();

                connection.Update(entity);
            }
        }

        public void UpdateProgress(long jobRunId, double? progress)
        {
            using (var connection = _connectionFactory.Open())
            {
                connection.Update<Entities.JobRun>(new { Progress = progress }, p => p.Id == jobRunId);
            }
        }

        public JobRun GetLastJobRunByTriggerId(long jobId, long triggerId, DateTime utcNow)
        {
            using (var connection = _connectionFactory.Open())
            {
                return connection.Select<JobRunInfo>(
                        connection.From<Entities.JobRun>()
                            .Join<Entities.JobRun, Trigger>((jr, t) => jr.TriggerId == t.Id)
                            .Join<Entities.JobRun, Entities.Job>((jr, j) => jr.JobId == j.Id)
                            .Where(p => p.TriggerId == triggerId && p.JobId == jobId)
                            .Where(p => p.PlannedStartDateTimeUtc >= utcNow)
                            .Where(p => p.Deleted == false)
                            .OrderBy(o => o.PlannedStartDateTimeUtc))
                            .Take(1)
                    .Select(s => s.ToModel())
                    .FirstOrDefault();
            }
        }

        public JobRun GetNextJobRunByTriggerId(long jobId, long triggerId, DateTime utcNow)
        {
            using (var connection = _connectionFactory.Open())
            {
                return connection.Select<JobRunInfo>(
                        connection.From<Entities.JobRun>()
                            .Join<Entities.JobRun, Trigger>((jr, t) => jr.TriggerId == t.Id)
                            .Join<Entities.JobRun, Entities.Job>((jr, j) => jr.JobId == j.Id)
                            .Where(p => p.TriggerId == triggerId && p.JobId == jobId)
                            .Where(p => p.State == JobRunStates.Scheduled)
                            .Where(p => p.PlannedStartDateTimeUtc >= utcNow)
                            .Where(p => p.Deleted == false)
                            .OrderBy(o => o.PlannedStartDateTimeUtc))
                    .Take(1)
                    .Select(s => s.ToModel())
                    .FirstOrDefault();
            }
        }

        public PagedResult<JobRun> GetJobRuns(int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null,
            string query = null, bool showDeleted = false, params string[] sort)
        {
            AssertOnlyOneFilterIsActive(jobTypeFilter, jobUniqueNameFilter, query);

            return GetJobRuns(null, page, pageSize, jobTypeFilter, jobUniqueNameFilter, query, showDeleted, sort);
        }

        public PagedResult<JobRun> GetJobRunsByJobId(int jobId, int page = 1, int pageSize = 50, bool showDeleted = false, params string[] sort)
        {
            return GetJobRuns(sql => sql.Where(p => p.JobId == jobId), page, pageSize, showDeleted: showDeleted, sort: sort);
        }

        public PagedResult<JobRun> GetJobRunsByUserId(string userId, int page = 1, int pageSize = 50, string jobTypeFilter = null,
            string jobUniqueNameFilter = null, bool showDeleted = false, params string[] sort)
        {
            return GetJobRuns(sql => sql.And<Trigger>(p => p.UserId == userId), page, pageSize, jobTypeFilter, jobUniqueNameFilter, null, showDeleted, sort);
        }

        public PagedResult<JobRun> GetJobRunsByTriggerId(long jobId, long triggerId, int page = 1, int pageSize = 50, bool showDeleted = false, params string[] sort)
        {
            return GetJobRuns(sql => sql.Where(p => p.TriggerId == triggerId && p.JobId == jobId), page, pageSize, showDeleted: showDeleted, sort: sort);
        }

        public PagedResult<JobRun> GetJobRunsByUserDisplayName(string userDisplayName, int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, bool showDeleted = false, params string[] sort)
        {
            return GetJobRuns(p => p.And<Trigger>(t => t.UserDisplayName == userDisplayName), page, pageSize, jobTypeFilter, jobUniqueNameFilter, showDeleted: showDeleted, sort: sort);
        }

        public PagedResult<JobRun> GetJobRunsByState(JobRunStates state, int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, string query = null, bool showDeleted = false, params string[] sort)
        {
            return GetJobRuns(sql => sql.Where(p => p.State == state), page, pageSize, jobTypeFilter, jobUniqueNameFilter, query, showDeleted, sort);
        }

        public PagedResult<JobRun> GetJobRunsByStates(JobRunStates[] states, int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, string query = null, bool showDeleted = false, params string[] sort)
        {
            return GetJobRuns(sql => sql.Where(p => states.Contains(p.State)), page, pageSize, jobTypeFilter, jobUniqueNameFilter, query, showDeleted, sort);
        }

        private PagedResult<JobRun> GetJobRuns(Action<SqlExpression<Entities.JobRun>> sql, int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, string query = null, bool showDeleted = false, params string[] sort)
        {
            using (var connection = _connectionFactory.Open())
            {
                var sqlExpression = connection.From<Entities.JobRun>()
                    .Join<Entities.JobRun, Trigger>((jr, t) => jr.TriggerId == t.Id)
                    .Join<Entities.JobRun, Entities.Job>((jr, j) => jr.JobId == j.Id)
                    .And<Entities.JobRun>(p => p.Deleted == showDeleted);

                if (jobTypeFilter != null)
                {
                    sqlExpression.And<Entities.Job>(p => p.Type == jobTypeFilter);
                }
                else if (jobUniqueNameFilter != null)
                {
                    sqlExpression.And<Entities.Job>(p => p.UniqueName == jobUniqueNameFilter);
                }
                else if (query != null)
                {
                    sqlExpression.And<Entities.Job>(p => p.UniqueName.Contains(query) || p.Type.Contains(query) || p.Title.Contains(query));
                }

                sql?.Invoke(sqlExpression);

                var count = connection.Count(sqlExpression);

                sqlExpression.Ordered(sort);

                sqlExpression.Skip((page - 1) * pageSize).Take(pageSize);

                var rows = connection.Select<JobRunInfo>(sqlExpression)
                    .Select(s => s.ToModel())
                    .ToList();

                return new PagedResult<JobRun>
                {
                    Items = rows,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = (int)count,
                };
            }
        }

        public void AddTrigger(long jobId, InstantTrigger trigger)
        {
            trigger.JobId = jobId;

            InsertTrigger(trigger);
        }

        public void AddTrigger(long jobId, ScheduledTrigger trigger)
        {
            trigger.JobId = jobId;

            InsertTrigger(trigger);
        }

        public void AddTrigger(long jobId, RecurringTrigger trigger)
        {
            trigger.JobId = jobId;

            InsertTrigger(trigger);
        }

        public void DeleteTrigger(long jobId, long triggerId)
        {
            using (var connection = _connectionFactory.Open())
            {
                connection.DeleteById<Trigger>(triggerId);
            }
        }

        public void Update(long jobId, InstantTrigger trigger)
        {
            using (var connection = _connectionFactory.Open())
            {
                var entity = trigger.ToEntity();

                connection.Update(entity);
            }
        }

        public void Update(long jobId, ScheduledTrigger trigger)
        {
            using (var connection = _connectionFactory.Open())
            {
                var entity = trigger.ToEntity();

                connection.Update(entity);
            }
        }

        public void Update(long jobId, RecurringTrigger trigger)
        {
            using (var connection = _connectionFactory.Open())
            {
                var entity = trigger.ToEntity();

                connection.Update(entity);
            }
        }

        public PagedResult<JobTriggerBase> GetActiveTriggers(
            int page = 1,
            int pageSize = 50,
            string jobTypeFilter = null,
            string jobUniqueNameFilter = null,
            string query = null,
            params string[] sort)
        {
            using (var connection = _connectionFactory.Open())
            {
                var sqlExpression = connection.From<Trigger>()
                    .Join<Entities.Job>()
                    .And<Entities.Job>(p => p.Deleted == false)
                    .Where(p => p.IsActive && p.Deleted == false);

                if (jobTypeFilter != null)
                {
                    sqlExpression.And<Job>(p => p.Type == jobTypeFilter);
                }
                else if (jobUniqueNameFilter != null)
                {
                    sqlExpression.And<Job>(p => p.UniqueName == jobUniqueNameFilter);
                }
                else if (query != null)
                {
                    sqlExpression.And<Job>(p => p.UniqueName.Contains(query) || p.Type.Contains(query) || p.Title.Contains(query));
                }

                var count = connection.Count(sqlExpression);

                sqlExpression.Skip((page - 1) * pageSize).Take(pageSize);

                var rows = connection.Select(sqlExpression)
                    .ToList()
                    .Select(s => s.ToModel())
                    .ToList();

                return new PagedResult<JobTriggerBase>
                {
                    Page = page,
                    PageSize = pageSize,
                    Items = rows,
                    TotalItems = (int)count,
                };
            }
        }

        public void DisableTrigger(long jobId, long triggerId)
        {
            using (var connection = _connectionFactory.Open())
            {
                connection.Update<Trigger>(new { IsActive = false }, p => p.Id == triggerId);
            }
        }

        public void EnableTrigger(long jobId, long triggerId)
        {
            using (var connection = _connectionFactory.Open())
            {
                connection.Update<Trigger>(new { IsActive = true }, p => p.Id == triggerId);
            }
        }

        public JobTriggerBase GetTriggerById(long jobId, long triggerId)
        {
            using (var connection = _connectionFactory.Open())
            {
                return connection.Select<Trigger>(p => p.JobId == jobId && p.Id == triggerId)
                    .FirstOrDefault()?
                    .ToModel();
            }
        }

        public PagedResult<JobTriggerBase> GetTriggersByJobId(long jobId, int page = 1, int pageSize = 50, bool showDeleted = false)
        {
            using (var connection = _connectionFactory.Open())
            {
                var sqlExpression = connection.From<Trigger>()
                    .Where(p => p.JobId == jobId)
                    .Where(p => p.Deleted == showDeleted)
                    .OrderByDescending(o => o.IsActive);

                var count = connection.Count(sqlExpression);

                sqlExpression.Skip((page - 1) * pageSize).Take(pageSize);

                var rows = connection.Select(sqlExpression)
                    .ToList()
                    .Select(s => s.ToModel())
                    .ToList();

                return new PagedResult<JobTriggerBase>
                {
                    Page = page,
                    PageSize = pageSize,
                    Items = rows,
                    TotalItems = (int)count,
                };
            }
        }

        private void InsertTrigger(InstantTrigger trigger)
        {
            var entity = trigger.ToEntity();

            if (entity.CreatedDateTimeUtc == default(DateTime))
            {
                entity.CreatedDateTimeUtc = DateTime.UtcNow;
            }

            using (var connection = _connectionFactory.OpenDbConnection())
            {
                trigger.Id = connection.Insert(entity, true);
            }
        }

        private void InsertTrigger(ScheduledTrigger trigger)
        {
            var entity = trigger.ToEntity();

            if (entity.CreatedDateTimeUtc == default(DateTime))
            {
                entity.CreatedDateTimeUtc = DateTime.UtcNow;
            }

            using (var connection = _connectionFactory.OpenDbConnection())
            {
                trigger.Id = connection.Insert(entity, true);
            }
        }

        private void InsertTrigger(RecurringTrigger trigger)
        {
            var entity = trigger.ToEntity();

            if (entity.CreatedDateTimeUtc == default(DateTime))
            {
                entity.CreatedDateTimeUtc = DateTime.UtcNow;
            }

            using (var connection = _connectionFactory.OpenDbConnection())
            {
                trigger.Id = connection.Insert(entity, true);
            }
        }

        public bool IsAvailable()
        {
            try
            {
                GetJobs(1, 1);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void ApplyRetention(DateTimeOffset deadline)
        {
            using (var connection = _connectionFactory.Open())
            {
                var triggerIds = connection.Select<Entities.JobRun>(p => p.PlannedStartDateTimeUtc <= deadline.UtcDateTime).Select(s => s.TriggerId).ToList();

                connection.Delete<Entities.JobRun>(p => p.PlannedStartDateTimeUtc <= deadline.UtcDateTime);

                // also delete orphaned scheduled/instant triggers
                connection.Delete<Entities.Trigger>(p => (p.Type == TriggerType.InstantTrigger || p.Type == TriggerType.ScheduledTrigger) && Sql.In(p.Id, triggerIds));
            }
        }

        private static void AssertOnlyOneFilterIsActive(string jobTypeFilter, string jobUniqueNameFilter, string query)
        {
            var sum = IsParameterSet(jobTypeFilter) + IsParameterSet(jobUniqueNameFilter) + IsParameterSet(query);

            if (sum > 1)
            {
                throw new InvalidOperationException("Only one filter is allowed");
            }

            int IsParameterSet(string input)
            {
                return input == null ? 0 : 1;
            }
        }
    }
}