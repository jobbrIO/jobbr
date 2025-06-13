using System;
using System.Collections.Generic;
using System.Linq;
using Jobbr.ComponentModel.Execution.Model;

namespace Jobbr.Server.ForkedExecution.Tests.Infrastructure
{
    public class FakeGeneratedJobRunsStore
    {
        private readonly List<FakeJobRunStoreTuple> _store = new();
        private readonly object _syncRoot = new();

        /// <summary>
        /// Create a fake job run.
        /// </summary>
        /// <param name="plannedStartDateTimeUtc">Planned start time in UTC.</param>
        /// <returns>A fake job run.</returns>
        public FakeJobRunStoreTuple CreateFakeJobRun(DateTime plannedStartDateTimeUtc)
        {
            long id;
            lock (_syncRoot)
            {
                id = _store.Any() ? _store.Max(e => e.Id) + 1 : 1;
            }

            var fakeJobRun = new FakeJobRunStoreTuple
            {
                Id = id,
                PlannedJobRun = new PlannedJobRun
                {
                    PlannedStartDateTimeUtc = plannedStartDateTimeUtc,
                    Id = id
                },
                JobRunInfo = new JobRunInfo
                {
                    Id = id,
                    JobId = new Random().Next(1, int.MaxValue),
                    TriggerId = new Random().Next(1, int.MaxValue),
                }
            };

            lock (_syncRoot)
            {
                _store.Add(fakeJobRun);
            }

            return fakeJobRun;
        }

        /// <summary>
        /// Get job run by ID.
        /// </summary>
        /// <param name="id">Job run ID.</param>
        /// <returns>Fake job run.</returns>
        public FakeJobRunStoreTuple GetByJobRunId(long id)
        {
            lock (_syncRoot)
            {
                return _store.SingleOrDefault(e => e.Id == id);
            }
        }

        internal FakeJobRunStoreTuple CreateFakeJobRun()
        {
            return CreateFakeJobRun(DateTime.UtcNow);
        }
    }
}