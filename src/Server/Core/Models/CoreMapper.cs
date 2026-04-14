using ForgeMap;
using Jobbr.ComponentModel.ArtefactStorage.Model;
using Jobbr.ComponentModel.JobStorage.Model;

namespace Jobbr.Server.Core.Models
{
    /// <summary>
    /// ForgeMap mapper for core model mappings.
    /// </summary>
    [ForgeMap]
    internal partial class CoreMapper
    {
        // Internal model → Storage entity
        public partial RecurringTrigger ForgeRecurringTrigger(RecurringTriggerModel source);

        public partial ScheduledTrigger ForgeScheduledTrigger(ScheduledTriggerModel source);

        public partial InstantTrigger ForgeInstantTrigger(InstantTriggerModel source);

        public partial Job ForgeJob(JobModel source);

        // Storage → Internal model
        public partial JobArtefactModel ForgeArtefactModel(JobbrArtefact source);

        public partial System.Collections.Generic.List<JobArtefactModel> ForgeArtefactModels(System.Collections.Generic.List<JobbrArtefact> source);
    }
}
