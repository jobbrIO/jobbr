﻿using System;
using System.Collections.Generic;
using System.Linq;
using Jobbr.ComponentModel.Execution;
using Jobbr.ComponentModel.Execution.Model;
using Jobbr.ComponentModel.JobStorage.Model;
using Jobbr.Server.Logging;
using Jobbr.Server.Scheduling.Planer;
using Jobbr.Server.Storage;

namespace Jobbr.Server.Scheduling
{
    public class NewScheduler : IJobScheduler
    {
        private static readonly ILog Logger = LogProvider.For<NewScheduler>();

        private readonly IJobbrRepository repository;
        private readonly IJobExecutor executor;

        private readonly InstantJobRunPlaner instantJobRunPlaner;
        private readonly ScheduledJobRunPlaner scheduledJobRunPlaner;
        private readonly RecurringJobRunPlaner recurringJobRunPlaner;
        private readonly DefaultSchedulerConfiguration configuration;

        private List<ScheduledPlanItem> currentPlan = new List<ScheduledPlanItem>();

        public NewScheduler(IJobbrRepository repository, IJobExecutor executor, InstantJobRunPlaner instantJobRunPlaner, ScheduledJobRunPlaner scheduledJobRunPlaner, RecurringJobRunPlaner recurringJobRunPlaner, DefaultSchedulerConfiguration configuration)
        {
            this.repository = repository;
            this.executor = executor;

            this.instantJobRunPlaner = instantJobRunPlaner;
            this.scheduledJobRunPlaner = scheduledJobRunPlaner;
            this.recurringJobRunPlaner = recurringJobRunPlaner;

            this.configuration = configuration;
        }

        public void Dispose()
        {
        }

        public void Start()
        {
            this.CreateInitialPlan();
        }

        public void Stop()
        {
        }

        public void OnTriggerDefinitionUpdated(long triggerId)
        {
            Logger.Info($"The trigger with id '{triggerId}' has been updated. Reflecting changes to Plan if any.");

            var trigger = this.repository.GetTriggerById(triggerId);

            PlanResult planResult = this.GetPlanResult(trigger as dynamic, false);

            if (planResult.Action != PlanAction.Possible)
            {
                Logger.Debug($"The trigger was not considered to be relevant to the plan, skipping. PlanResult was '{planResult.Action}'");
                return;
            }

            var dateTime = planResult.ExpectedStartDateUtc;

            if (!dateTime.HasValue)
            {
                Logger.Warn($"Unable to gather an expected start date for trigger, skipping.");
                return;
            }

            // Get the next occurence from database
            var dependentJobRun = this.repository.GetNextJobRunByTriggerId(trigger.Id);

            if (dependentJobRun == null)
            {
                Logger.Error($"Trigger was updated before job run has been created. Cannot apply update.");
                return;
            }

            this.UpdatePlannedJobRun(dependentJobRun, trigger, dateTime.Value);
        }

        public void OnTriggerStateUpdated(long triggerId)
        {
            Logger.Info($"The trigger with id '{triggerId}' has been changed its state. Reflecting changes to Plan if any.");

            var trigger = this.repository.GetTriggerById(triggerId);

            PlanResult planResult = this.GetPlanResult(trigger as dynamic, false);

            if (planResult.Action == PlanAction.Obsolete)
            {
                // Remove from in memory plan to not publish this in future
                this.currentPlan.RemoveAll(e => e.TriggerId == triggerId);

                // Set the JobRun to deleted if any
                var dependentJobRun = this.repository.GetNextJobRunByTriggerId(trigger.Id);

                if (dependentJobRun != null)
                {
                    this.repository.Delete(dependentJobRun);
                }

                this.PublishCurrentPlan();

                return;
            }

            if (planResult.Action == PlanAction.Possible)
            {
                var newItem = this.CreateNew(planResult, trigger);

                if (newItem != null)
                {
                    this.currentPlan.Add(newItem);

                    this.PublishCurrentPlan();
                }
            }
        }

        public void OnTriggerAdded(long triggerId)
        {
            Logger.Info($"The trigger with id '{triggerId}' has been added. Reflecting changes to the current plan.");

            var trigger = this.repository.GetTriggerById(triggerId);

            PlanResult planResult = this.GetPlanResult(trigger as dynamic, true);

            if (planResult.Action != PlanAction.Possible)
            {
                Logger.Debug($"The trigger was not considered to be relevant to the plan, skipping. PlanResult was '{planResult.Action}'");
                return;
            }

            var newItem = this.CreateNew(planResult, trigger);

            if (newItem == null)
            {
                Logger.Error($"Unable to create a new Planned Item with a JobRun.");
                return;
            }

            this.currentPlan.Add(newItem);

            this.PublishCurrentPlan();
        }

        public void OnJobRunEnded(Guid uniqueId)
        {
            Logger.Info($"A JobRun has ended. Reevaluating triggers that did not yet schedule a run");

            // Remove from in memory plan to not publish this in future
            this.currentPlan.RemoveAll(e => e.UniqueId == uniqueId);

            var additonalItems = new List<ScheduledPlanItem>();

            // If a trigger was blocked previously, it might be a candidate to schedule now
            var activeTriggers = this.repository.GetActiveTriggers();

            foreach (var trigger in activeTriggers)
            {
                if (this.currentPlan.Any(p => p.TriggerId == trigger.Id))
                {
                    continue;
                }

                PlanResult planResult = this.GetPlanResult(trigger as dynamic, false);

                if (planResult.Action == PlanAction.Possible)
                {
                    var scheduledItem = this.CreateNew(planResult, trigger);

                    additonalItems.Add(scheduledItem);
                }
            }

            if (additonalItems.Any())
            {
                this.currentPlan.AddRange(additonalItems);
                Logger.Info($"The completion of a previous job caused {additonalItems.Count} scheduled items");

                this.PublishCurrentPlan();
            }
            else
            {
                Logger.Debug($"There was no possibility to scheduled new items after the completion of job with it '{uniqueId}'.");
            }
        }

        private ScheduledPlanItem CreateNew(PlanResult planResult, JobTriggerBase trigger)
        {
            var dateTime = planResult.ExpectedStartDateUtc;

            if (!dateTime.HasValue)
            {
                Logger.Warn($"Unable to gather an expected start date for trigger, skipping.");

                return null;
            }

            // Create the next occurence from database
            var newJobRun = this.CreateNewJobRun(trigger, dateTime.Value);

            // Add to the initial plan
            var newItem = new ScheduledPlanItem
            {
                TriggerId = trigger.Id,
                UniqueId = newJobRun.UniqueId,
                PlannedStartDateTimeUtc = newJobRun.PlannedStartDateTimeUtc
            };

            return newItem;
        }

        private void CreateInitialPlan()
        {
            var activeTriggers = this.repository.GetActiveTriggers();

            var newPlan = new List<ScheduledPlanItem>();
            foreach (var trigger in activeTriggers)
            {
                PlanResult planResult = this.GetPlanResult(trigger as dynamic, false);

                if (planResult.Action == PlanAction.Obsolete)
                {
                    Logger.WarnFormat($"Disabling trigger with id '{trigger.Id}', because startdate is in the past. (Type: '{trigger.GetType().Name}', userId: '{trigger.UserId}', userName: '{trigger.UserName}')");

                    this.repository.DisableTrigger(trigger.Id);
                    continue;
                }

                if (planResult.Action == PlanAction.Blocked)
                {
                    // Cannot schedule jobrun, one reason could be that this job is not allowed to run because another jobrun is active
                    continue;
                }

                if (planResult.Action == PlanAction.Possible)
                {
                    if (planResult.ExpectedStartDateUtc == null)
                    {
                        // Move to ctor of PlanResult
                        throw new ArgumentNullException("ExpectedStartDateUtc");
                    }

                    var dateTime = planResult.ExpectedStartDateUtc;

                    // Get the next occurence from database
                    var dependentJobRun = this.repository.GetNextJobRunByTriggerId(trigger.Id);

                    if (dependentJobRun != null)
                    {
                        this.UpdatePlannedJobRun(dependentJobRun, trigger, dateTime.Value);
                    }
                    else
                    {
                        dependentJobRun = this.CreateNewJobRun(trigger, dateTime.Value);
                    }

                    // Add to the initial plan
                    newPlan.Add(new ScheduledPlanItem()
                    {
                        TriggerId = trigger.Id,
                        UniqueId = dependentJobRun.UniqueId,
                        PlannedStartDateTimeUtc = dependentJobRun.PlannedStartDateTimeUtc
                    });
                }
            }

            // Set current plan
            this.currentPlan = newPlan;

            // Publish the initial plan top the Excutor
            this.PublishCurrentPlan();
        }

        private void PublishCurrentPlan()
        {
            Logger.Info($"Publishing new plan for upcoming jobs to the executor. Number of Items: {this.currentPlan.Count}");

            var clone = this.currentPlan.Select(e => new PlannedJobRun() { PlannedStartDateTimeUtc = e.PlannedStartDateTimeUtc, UniqueId = e.UniqueId }).ToList();

            try
            {
                this.executor.OnPlanChanged(clone);
            }
            catch (Exception e)
            {
                Logger.WarnException("Unable to publish current plan to Executor", e);
            }
        }

        private JobRun CreateNewJobRun(JobTriggerBase trigger, DateTime dateTime)
        {
            var job = this.repository.GetJob(trigger.JobId);

            var jobRun = this.repository.SaveNewJobRun(job, trigger, dateTime);

            return jobRun;
        }

        private void UpdatePlannedJobRun(JobRun plannedNextRun, JobTriggerBase trigger, DateTime calculatedNextRun)
        {
            // Is this value in sync with the schedule table?
            if (plannedNextRun.PlannedStartDateTimeUtc == calculatedNextRun)
            {
                Logger.DebugFormat(
                    "The previously planned startdate '{0}' is still correct for JobRun (id: {1}) triggered by trigger with id '{2}' (Type: '{3}', userId: '{4}', userName: '{5}')",
                    calculatedNextRun,
                    plannedNextRun.Id,
                    trigger.Id,
                    trigger.GetType().Name,
                    trigger.UserId,
                    trigger.UserName);

                return;
            }

            // Was the change too close before the execution date?
            if (DateTime.UtcNow.AddSeconds(this.configuration.AllowChangesBeforeStartInSec) >= calculatedNextRun)
            {
                Logger.WarnFormat(
                    "The planned startdate '{0}' has changed to '{1}'. This change was done too close (less than {2} seconds) to the next planned run and cannot be adjusted",
                    plannedNextRun.PlannedStartDateTimeUtc,
                    calculatedNextRun,
                    this.configuration.AllowChangesBeforeStartInSec);

                return;
            }

            Logger.WarnFormat("The calculated startdate '{0}' has changed to '{1}', the planned jobRun needs to be updated as next step", plannedNextRun.PlannedStartDateTimeUtc, calculatedNextRun);

            plannedNextRun.PlannedStartDateTimeUtc = calculatedNextRun;
            this.repository.Update(plannedNextRun);
        }

        private PlanResult GetPlanResult(InstantTrigger trigger, bool isNew = false) => this.instantJobRunPlaner.Plan(trigger, isNew);

        // ReSharper disable once UnusedParameter.Local
        // Reason: Parameter is used by dynamic overload
        private PlanResult GetPlanResult(ScheduledTrigger trigger, bool isNew = false) => this.scheduledJobRunPlaner.Plan(trigger);

        // ReSharper disable once UnusedParameter.Local
        // Reason: Parameter is used by dynamic overload
        private PlanResult GetPlanResult(RecurringTrigger trigger, bool isNew = false) => this.recurringJobRunPlaner.Plan(trigger);

        // ReSharper disable once UnusedParameter.Local
        // Reason: Parameter is used by dynamic overload
        private PlanResult GetPlanResult(object trigger, bool isNew)
        {
            throw new NotImplementedException($"Unable to dynamic dispatch trigger of type '{trigger?.GetType().Name}'");
        }
    }
}
