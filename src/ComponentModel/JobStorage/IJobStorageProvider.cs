using System;
using Jobbr.ComponentModel.JobStorage.Model;

namespace Jobbr.ComponentModel.JobStorage
{
    public interface IJobStorageProvider
    {
        #region Jobs

        void AddJob(Job job);

        void DeleteJob(long jobId);

        long GetJobsCount();

        PagedResult<Job> GetJobs(int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, string query = null, bool showDeleted = false, params string[] sort);

        Job GetJobById(long id);

        Job GetJobByUniqueName(string identifier);

        void Update(Job job);

        #endregion

        #region Triggers

        void AddTrigger(long jobId, RecurringTrigger trigger);

        void AddTrigger(long jobId, InstantTrigger trigger);

        void AddTrigger(long jobId, ScheduledTrigger trigger);

        JobTriggerBase GetTriggerById(long jobId, long triggerId);

        PagedResult<JobTriggerBase> GetTriggersByJobId(long jobId, int page = 1, int pageSize = 50, bool showDeleted = false);

        PagedResult<JobTriggerBase> GetActiveTriggers(int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, string query = null, params string[] sort);

        void DisableTrigger(long jobId, long triggerId);

        void EnableTrigger(long jobId, long triggerId);

        void DeleteTrigger(long jobId, long triggerId);

        void Update(long jobId, InstantTrigger trigger);

        void Update(long jobId, ScheduledTrigger trigger);

        void Update(long jobId, RecurringTrigger trigger);

        #endregion

        #region Jobruns

        void AddJobRun(JobRun jobRun);

        JobRun GetJobRunById(long id);

        JobRun GetLastJobRunByTriggerId(long jobId, long triggerId, DateTime utcNow);

        JobRun GetNextJobRunByTriggerId(long jobId, long triggerId, DateTime utcNow);

        PagedResult<JobRun> GetJobRuns(int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, string query = null, bool showDeleted = false, params string[] sort);

        PagedResult<JobRun> GetJobRunsByJobId(int jobId, int page = 1, int pageSize = 50, bool showDeleted = false, params string[] sort);

        PagedResult<JobRun> GetJobRunsByUserId(string userId, int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, bool showDeleted = false, params string[] sort);

        PagedResult<JobRun> GetJobRunsByTriggerId(long jobId, long triggerId, int page = 1, int pageSize = 50, bool showDeleted = false, params string[] sort);

        PagedResult<JobRun> GetJobRunsByUserDisplayName(string userDisplayName, int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, bool showDeleted = false, params string[] sort);

        PagedResult<JobRun> GetJobRunsByState(JobRunStates state, int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, string query = null, bool showDeleted = false, params string[] sort);

        PagedResult<JobRun> GetJobRunsByStates(JobRunStates[] states, int page = 1, int pageSize = 50, string jobTypeFilter = null, string jobUniqueNameFilter = null, string query = null, bool showDeleted = false, params string[] sort);

        void Update(JobRun jobRun);

        void UpdateProgress(long jobRunId, double? progress);

        #endregion

        bool IsAvailable();
    }
}