using System.Text.Json.Serialization;

namespace Jobbr.Server.WebAPI.Model
{
    /// <summary>
    /// The job trigger base
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "triggerType")]
    [JsonDerivedType(typeof(InstantTriggerDto), typeDiscriminator: InstantTriggerDto.Type)]
    [JsonDerivedType(typeof(RecurringTriggerDto), typeDiscriminator: RecurringTriggerDto.Type)]
    [JsonDerivedType(typeof(ScheduledTriggerDto), typeDiscriminator: ScheduledTriggerDto.Type)]
    public class JobTriggerDtoBase
    {
        /// <summary>
        /// Job Trigger ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Trigger Type overwritten by derived types
        /// </summary>
        public virtual string TriggerType => string.Empty;

        /// <summary>
        /// Determines if the Trigger is active or not
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Custom instance of variable parameters for the Trigger
        /// </summary>
        public object Parameters { get; set; }

        /// <summary>
        /// Documentation or audit comment for the Trigger
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// ID of the User who created the Trigger
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Name of the User who created the Trigger
        /// </summary>
        public string UserDisplayName { get; set; }

        /// <summary>
        /// Indicator whether the Trigger was soft-deleted
        /// </summary>
        public bool Deleted { get; set; }
    }
}
