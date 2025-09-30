import { JobTriggerDto, JobTriggerDtoBase, InstantTriggerDto, ScheduledTriggerDto, RecurringTriggerDto } from '../types';

// Union type for trigger type selection
export type TriggerType = 'Instant' | 'Scheduled' | 'Recurring';

// Type guards for trigger types
export const isInstantTrigger = (trigger: JobTriggerDto): trigger is InstantTriggerDto => 
  trigger.triggerType === 'Instant';

export const isScheduledTrigger = (trigger: JobTriggerDto): trigger is ScheduledTriggerDto => 
  trigger.triggerType === 'Scheduled';

export const isRecurringTrigger = (trigger: JobTriggerDto): trigger is RecurringTriggerDto => 
  trigger.triggerType === 'Recurring';

// Helper function to get trigger type-specific properties safely
export const getTriggerTypeProperties = (trigger: JobTriggerDto) => {
  if (isInstantTrigger(trigger)) {
    return {
      type: 'Instant' as const,
      delayedMinutes: trigger.delayedMinutes,
    };
  }
  
  if (isScheduledTrigger(trigger)) {
    return {
      type: 'Scheduled' as const,
      startDateTimeUtc: trigger.startDateTimeUtc,
    };
  }
  
  if (isRecurringTrigger(trigger)) {
    return {
      type: 'Recurring' as const,
      definition: trigger.definition,
      startDateTimeUtc: trigger.startDateTimeUtc,
      endDateTimeUtc: trigger.endDateTimeUtc,
    };
  }
  
  // This should never happen if the discriminated union is properly typed
  throw new Error(`Unknown trigger type: ${(trigger as JobTriggerDtoBase).triggerType}`);
};

// Utility function to validate trigger type at runtime
export const isValidTriggerType = (type: string): type is TriggerType => {
  return ['Instant', 'Scheduled', 'Recurring'].includes(type);
};