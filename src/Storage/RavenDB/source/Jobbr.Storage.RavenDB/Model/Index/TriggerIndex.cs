using System;
using System.Linq;
using Raven.Client.Indexes;

namespace Jobbr.Storage.RavenDB.Model.Index
{
    public class TriggerIndex : AbstractMultiMapIndexCreationTask<Job>
    {
        public class Result
        {
            public string JobId { get; set; }
            public long TriggerId { get; set; }
            public string UniqueName { get; set; }
            public string Type { get; set; }
            public bool IsActive { get; set; }
            public string Title { get; set; }
            public string UserId { get; set; }
            public string UserDisplayName { get; set; }
            public DateTime? JobCreateDateTimeUtc { get; set; }
            public DateTime TriggerCreateDateTimeUtc { get; set; }
        }

        public TriggerIndex()
        {
            AddMap<Job>(jobs => from job in jobs
                                from instantTrigger in job.InstantTriggers
                                select
                                new Result
                                {
                                    UniqueName = job.UniqueName,
                                    Type = job.Type,
                                    JobCreateDateTimeUtc = job.CreatedDateTimeUtc,
                                    Title = job.Title,
                                    JobId = job.Id,
                                    TriggerId = instantTrigger.Id,
                                    IsActive = instantTrigger.IsActive,
                                    UserId = instantTrigger.UserId,
                                    UserDisplayName = instantTrigger.UserDisplayName,
                                    TriggerCreateDateTimeUtc = instantTrigger.CreatedDateTimeUtc
                                });

            AddMap<Job>(jobs => from job in jobs
                                from scheduledTrigger in job.ScheduledTriggers
                                select
                                new Result
                                {
                                    UniqueName = job.UniqueName,
                                    Type = job.Type,
                                    JobCreateDateTimeUtc = job.CreatedDateTimeUtc,
                                    Title = job.Title,
                                    JobId = job.Id,
                                    TriggerId = scheduledTrigger.Id,
                                    IsActive = scheduledTrigger.IsActive,
                                    UserId = scheduledTrigger.UserId,
                                    UserDisplayName = scheduledTrigger.UserDisplayName,
                                    TriggerCreateDateTimeUtc = scheduledTrigger.CreatedDateTimeUtc
                                });

            AddMap<Job>(jobs => from job in jobs
                                from recurringTrigger in job.RecurringTriggers
                                select
                                new Result
                                {
                                    UniqueName = job.UniqueName,
                                    Type = job.Type,
                                    JobCreateDateTimeUtc = job.CreatedDateTimeUtc,
                                    Title = job.Title,
                                    JobId = job.Id,
                                    TriggerId = recurringTrigger.Id,
                                    IsActive = recurringTrigger.IsActive,
                                    UserId = recurringTrigger.UserId,
                                    UserDisplayName = recurringTrigger.UserDisplayName,
                                    TriggerCreateDateTimeUtc = recurringTrigger.CreatedDateTimeUtc
                                });
        }
    }
}
