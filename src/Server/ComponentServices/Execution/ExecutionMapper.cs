using ForgeMap;
using Jobbr.ComponentModel.Execution.Model;
using Jobbr.ComponentModel.JobStorage.Model;

namespace Jobbr.Server.ComponentServices.Execution
{
    /// <summary>
    /// ForgeMap mapper for execution-related mappings.
    /// </summary>
    [ForgeMap]
    internal partial class ExecutionMapper
    {
        [ForgeProperty(nameof(Job.Id), nameof(JobRunInfo.JobId))]
        [ForgeProperty(nameof(Job.Parameters), nameof(JobRunInfo.JobParameters))]
        [Ignore(nameof(JobRunInfo.Id), nameof(JobRunInfo.InstanceParameters), nameof(JobRunInfo.TriggerId), nameof(JobRunInfo.UserDisplayName), nameof(JobRunInfo.UserId))]
        public partial void ForgeInto(Job source, [UseExistingValue] JobRunInfo destination);

        [Ignore(nameof(JobRunInfo.Type), nameof(JobRunInfo.UniqueName), nameof(JobRunInfo.UserId), nameof(JobRunInfo.UserDisplayName), nameof(JobRunInfo.JobId), nameof(JobRunInfo.TriggerId), nameof(JobRunInfo.JobParameters))]
        public partial void ForgeInto(JobRun source, [UseExistingValue] JobRunInfo destination);

        [ForgeProperty(nameof(JobTriggerBase.Id), nameof(JobRunInfo.TriggerId))]
        [Ignore(nameof(JobRunInfo.Type), nameof(JobRunInfo.UniqueName), nameof(JobRunInfo.JobParameters), nameof(JobRunInfo.InstanceParameters), nameof(JobRunInfo.Id), nameof(JobRunInfo.JobId))]
        public partial void ForgeInto(JobTriggerBase source, [UseExistingValue] JobRunInfo destination);
    }
}
