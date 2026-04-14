using System;
using ForgeMap;
using Jobbr.ComponentModel.JobStorage.Model;
using Jobbr.ComponentModel.Management.Model;
using Jobbr.Server.Core.Models;
using ManagementJob = Jobbr.ComponentModel.Management.Model.Job;
using ManagementJobRun = Jobbr.ComponentModel.Management.Model.JobRun;

namespace Jobbr.Server.ComponentServices.Management
{
    /// <summary>
    /// ForgeMap mapper for management-related mappings.
    /// </summary>
    [ForgeMap]
    internal partial class ManagementMapper
    {
        // Storage → Management model mappings
        public partial ManagementJob Forge(ComponentModel.JobStorage.Model.Job source);

        public partial ComponentModel.Management.Model.RecurringTrigger ForgeRecurringTrigger(ComponentModel.JobStorage.Model.RecurringTrigger source);

        public partial ComponentModel.Management.Model.ScheduledTrigger ForgeScheduledTrigger(ComponentModel.JobStorage.Model.ScheduledTrigger source);

        public partial ComponentModel.Management.Model.InstantTrigger ForgeInstantTrigger(ComponentModel.JobStorage.Model.InstantTrigger source);

        // Storage JobRun → Management JobRun (complex: nested nav property access)
        [ForgeProperty(nameof(ComponentModel.JobStorage.Model.JobRun.State), nameof(ManagementJobRun.State), ConvertWith = nameof(ConvertJobRunState))]
        [ForgeFrom(nameof(ManagementJobRun.TriggerType), nameof(GetTriggerType))]
        [ForgeFrom(nameof(ManagementJobRun.Definition), nameof(GetDefinition))]
        [ForgeFrom(nameof(ManagementJobRun.JobId), nameof(GetJobId))]
        [ForgeFrom(nameof(ManagementJobRun.JobName), nameof(GetJobName))]
        [ForgeFrom(nameof(ManagementJobRun.JobType), nameof(GetJobType))]
        [ForgeFrom(nameof(ManagementJobRun.TriggerId), nameof(GetTriggerId))]
        [ForgeFrom(nameof(ManagementJobRun.Comment), nameof(GetTriggerComment))]
        [ForgeFrom(nameof(ManagementJobRun.UserId), nameof(GetTriggerUserId))]
        [ForgeFrom(nameof(ManagementJobRun.UserDisplayName), nameof(GetTriggerUserDisplayName))]
        public partial ManagementJobRun ForgeJobRun(ComponentModel.JobStorage.Model.JobRun source);

        // Management → Internal model mappings
        [Ignore(nameof(RecurringTriggerModel.CreatedDateTimeUtc))]
        public partial RecurringTriggerModel ForgeRecurringTriggerModel(ComponentModel.Management.Model.RecurringTrigger source);

        [Ignore(nameof(ScheduledTriggerModel.CreatedDateTimeUtc))]
        public partial ScheduledTriggerModel ForgeScheduledTriggerModel(ComponentModel.Management.Model.ScheduledTrigger source);

        [Ignore(nameof(InstantTriggerModel.CreatedDateTimeUtc))]
        public partial InstantTriggerModel ForgeInstantTriggerModel(ComponentModel.Management.Model.InstantTrigger source);

        public partial JobModel ForgeJobModel(ComponentModel.Management.Model.Job source);

        // Internal → Management model mappings
        [ForgeProperty(nameof(JobArtefactModel.MimeType), nameof(JobArtefact.Type))]
        public partial JobArtefact ForgeJobArtefact(JobArtefactModel source);

        public partial System.Collections.Generic.List<JobArtefact> ForgeJobArtefacts(System.Collections.Generic.List<JobArtefactModel> source);

        // Converter helpers
        private static ComponentModel.Management.Model.JobRunStates ConvertJobRunState(ComponentModel.JobStorage.Model.JobRunStates state)
        {
            return (ComponentModel.Management.Model.JobRunStates)Enum.Parse(typeof(ComponentModel.Management.Model.JobRunStates), state.ToString());
        }

        private static string GetTriggerType(ComponentModel.JobStorage.Model.JobRun source)
        {
            return source.Trigger?.GetType().Name;
        }

        private static string GetDefinition(ComponentModel.JobStorage.Model.JobRun source)
        {
            return (source.Trigger as ComponentModel.JobStorage.Model.RecurringTrigger)?.Definition;
        }

        private static long GetJobId(ComponentModel.JobStorage.Model.JobRun source)
        {
            return source.Job?.Id ?? 0;
        }

        private static string GetJobName(ComponentModel.JobStorage.Model.JobRun source)
        {
            return source.Job?.UniqueName;
        }

        private static string GetJobType(ComponentModel.JobStorage.Model.JobRun source)
        {
            return source.Job?.Type;
        }

        private static long GetTriggerId(ComponentModel.JobStorage.Model.JobRun source)
        {
            return source.Trigger?.Id ?? 0;
        }

        private static string GetTriggerComment(ComponentModel.JobStorage.Model.JobRun source)
        {
            return source.Trigger?.Comment;
        }

        private static string GetTriggerUserId(ComponentModel.JobStorage.Model.JobRun source)
        {
            return source.Trigger?.UserId;
        }

        private static string GetTriggerUserDisplayName(ComponentModel.JobStorage.Model.JobRun source)
        {
            return source.Trigger?.UserDisplayName;
        }
    }
}
