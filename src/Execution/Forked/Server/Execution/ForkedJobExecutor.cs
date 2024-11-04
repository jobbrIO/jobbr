using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Jobbr.ComponentModel.Execution;
using Jobbr.ComponentModel.Execution.Model;
using Microsoft.Extensions.Logging;

namespace Jobbr.Server.ForkedExecution.Execution
{
    /// <summary>
    /// Executes parallel jobs.
    /// </summary>
    public class ForkedJobExecutor : IJobExecutor
    {
        private const long MaxWebHostStartWait = 2000;
        private const int StartNewJobsEverySeconds = 1;

        private readonly ILogger<ForkedJobExecutor> _logger;
        private readonly IJobRunContextFactory _jobRunContextFactory;
        private readonly IJobRunInformationService _jobRunInformationService;
        private readonly ForkedExecutionConfiguration _configuration;
        private readonly IJobRunProgressChannel _progressChannel;
        private readonly IPeriodicTimer _periodicTimer;
        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly List<PlannedJobRun> _plannedJobRuns = new ();
        private readonly List<IJobRunContext> _activeContexts = new ();

        private readonly object _syncRoot = new ();

        /// <summary>
        /// Initializes a new instance of the <see cref="ForkedJobExecutor"/> class.
        /// </summary>
        public ForkedJobExecutor(ILoggerFactory loggerFactory, IJobRunContextFactory jobRunContextFactory, IJobRunInformationService jobRunInformationService, IJobRunProgressChannel progressChannel, IPeriodicTimer periodicTimer, IDateTimeProvider dateTimeProvider, ForkedExecutionConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<ForkedJobExecutor>();
            _jobRunContextFactory = jobRunContextFactory;
            _jobRunInformationService = jobRunInformationService;
            _configuration = configuration;
            _progressChannel = progressChannel;
            _periodicTimer = periodicTimer;
            _dateTimeProvider = dateTimeProvider;

            _periodicTimer.Setup(StartReadyJobsFromQueue, StartNewJobsEverySeconds);
        }

        /// <inheritdoc/>>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts the executor.
        /// </summary>
        /// <exception cref="Exception">Unable to get information about the back channel.</exception>
        public void Start()
        {
            _logger.LogDebug("Starting ForkedJobExecutor asynchronously...");
            var duration = new Stopwatch();
            duration.Start();

            Task.Run(async () =>
            {
                while (string.IsNullOrWhiteSpace(_configuration.BackendAddress) && duration.ElapsedMilliseconds < MaxWebHostStartWait)
                {
                    await Task.Delay(200);
                }

                duration.Stop();

                if (string.IsNullOrWhiteSpace(_configuration.BackendAddress) || duration.ElapsedMilliseconds > MaxWebHostStartWait)
                {
                    throw new Exception("Unable to get information about the BackChannel Connection readiness. Executor will be in unhealthy state");
                }

                _logger.LogDebug("ForkedJobExecutor started. Backchannel will be available @ '{backendAddress}'", _configuration.BackendAddress);

                _logger.LogInformation("Enabling periodic check for JobRuns to start every {jobInterval}s", StartNewJobsEverySeconds);

                _periodicTimer.Start();
            });
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            _periodicTimer.Stop();
        }

        /// <summary>
        /// Initiates a new list of planned job runs.
        /// </summary>
        /// <param name="newPlan">List of new <see cref="PlannedJobRun"/>s.</param>
        public void OnPlanChanged(List<PlannedJobRun> newPlan)
        {
            var hadChanges = 0;

            lock (_syncRoot)
            {
                _logger.LogInformation("Got a plan with {jobRunCount} scheduled JobRuns with an upcoming start date.", newPlan.Count);

                // Update start dates of existing
                foreach (var plannedJobRun in newPlan)
                {
                    var existing = _plannedJobRuns.SingleOrDefault(e => e.Id == plannedJobRun.Id);

                    if (existing != null && existing.PlannedStartDateTimeUtc != plannedJobRun.PlannedStartDateTimeUtc)
                    {
                        existing.PlannedStartDateTimeUtc = plannedJobRun.PlannedStartDateTimeUtc;
                        hadChanges++;
                        _logger.LogInformation("Changed start date of job run '{existingId}' to '{plannedStartTime}'", existing.Id, plannedJobRun.PlannedStartDateTimeUtc);
                    }
                }

                // Add only new
                var toAdd = newPlan.Where(newItem => _plannedJobRuns.All(existingItem => existingItem.Id != newItem.Id) && _activeContexts.All(c => c.JobRunId != newItem.Id)).ToList();
                _plannedJobRuns.AddRange(toAdd);
                hadChanges += toAdd.Count;

                _logger.LogInformation("Added {addCount} new planned job runs based on the new plan", toAdd.Count);

                // Remove non existing
                var toRemove = _plannedJobRuns.Where(existingItem => newPlan.All(newItem => existingItem.Id != newItem.Id)).ToList();
                _plannedJobRuns.RemoveAll(p => toRemove.Contains(p));
                hadChanges += toRemove.Count;

                _logger.LogInformation("Removed {removeCount} previously planned job runs.", toRemove.Count);
            }

            if (hadChanges > 0)
            {
                // Immediately handle changes
                StartReadyJobsFromQueue();
            }
        }

        /// <summary>
        /// Handles job run cancellation.
        /// </summary>
        /// <param name="id">Job run ID.</param>
        /// <returns>Unimplemented.</returns>
        /// <exception cref="NotImplementedException">Not implemented.</exception>
        public bool OnJobRunCanceled(long id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method for disposing the object.
        /// </summary>
        /// <param name="disposing">If object is being disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
            }
        }

        private void StartReadyJobsFromQueue()
        {
            lock (_syncRoot)
            {
                var possibleJobsToStart = _configuration.MaxConcurrentProcesses - _activeContexts.Count;
                var readyJobs = _plannedJobRuns.Where(jr => jr.PlannedStartDateTimeUtc <= _dateTimeProvider.GetUtcNow()).OrderBy(jr => jr.PlannedStartDateTimeUtc).ToList();

                var jobsToStart = readyJobs.Take(possibleJobsToStart).ToList();

                var queueCannotStartAll = readyJobs.Count > possibleJobsToStart;
                var showStatusInformationNow = (DateTime.Now.Second % 5) == 0;
                var canStartAllReadyJobs = jobsToStart.Count > 0 && jobsToStart.Count <= possibleJobsToStart;

                if ((queueCannotStartAll && showStatusInformationNow) || canStartAllReadyJobs)
                {
                    _logger.LogInformation("There are {jobCount} planned jobs in the queue and currently {contextCount} running jobs. Number of possible jobs to start: {possibleJobsToStart}", readyJobs.Count, _activeContexts.Count, possibleJobsToStart);
                }

                foreach (var jobRun in jobsToStart)
                {
                    _logger.LogDebug("Trying to start job with Id '{jobRunId}' which was planned for {startTime}.", jobRun.Id, jobRun.PlannedStartDateTimeUtc);

                    IJobRunContext wrapper = null;

                    try
                    {
                        _logger.LogDebug("Getting Metadata for a job (ID: '{jobRunId}') that needs to be started.", jobRun.Id);
                        var jobRunInfo = _jobRunInformationService.GetByJobRunId(jobRun.Id);

                        wrapper = _jobRunContextFactory.CreateJobRunContext(jobRunInfo);

                        _activeContexts.Add(wrapper);
                        _plannedJobRuns.Remove(jobRun);

                        wrapper.Ended += ContextOnEnded;
                        wrapper.Start();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Exception was thrown while starting a new JobRun with ID: '{jobRunId}'.", jobRun.Id);

                        if (wrapper != null)
                        {
                            wrapper.Ended -= ContextOnEnded;
                        }
                    }
                }
            }
        }

        private void ContextOnEnded(object sender, JobRunEndedEventArgs args)
        {
            if (!(sender is IJobRunContext jobRunContext))
            {
                return;
            }

            JobRunInfo run;

            lock (_syncRoot)
            {
                jobRunContext.Ended -= ContextOnEnded;

                run = args.JobRun;
                _logger.LogInformation("Removing context for JobRun with ID: {runId} (TriggerID: {triggerId}, JobID: {jobId})", run.Id, run.TriggerId, run.JobId);

                _activeContexts.Remove(jobRunContext);
            }

            if (args.ExitCode == 0)
            {
                try
                {
                    _progressChannel.PublishStatusUpdate(run.Id, JobRunStates.Completed);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception while setting the end-time of the job run with ID: {runId} (TriggerID: {triggerId}, JobID: {jobId})", run.Id, run.TriggerId, run.JobId);
                }

                if (args.DidReportProgress)
                {
                    try
                    {
                        _progressChannel.PublishProgressUpdate(args.JobRun.Id, 100);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Exception while setting progress to 100% after completion of the job run with ID: {runId} (TriggerID: {triggerId}, JobID: {jobId})", run.Id, run.TriggerId, run.JobId);
                    }
                }
            }
            else
            {
                _logger.LogError("The process within the context JobRun has exited with a non-zero exit code. JobRunID: {runId} (TriggerID: {triggerId}, JobID: {jobId})", run.Id, run.TriggerId, run.JobId);

                try
                {
                    _progressChannel.PublishStatusUpdate(run.Id, JobRunStates.Failed);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception while setting the 'Failed'-State to the job run with ID: {runId} (TriggerID: {triggerId}, JobID: {JobId})", run.Id, run.TriggerId, run.JobId);
                }
            }
        }
    }
}