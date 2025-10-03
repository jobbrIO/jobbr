// API Data Transfer Objects - TypeScript interfaces
export interface JobDto {
  id: number;
  uniqueName: string;
  title: string;
  parameters?: string | object;
  type: string;
  createdDateTimeUtc: string;
  updatedDateTimeUtc?: string;
  trigger?: JobTriggerDtoBase[];
  deleted: boolean;
}

export interface JobRunArtefactDto {
  filename: string;
  size: number;
  contentType: string;
}

export interface JobRunDto {
  jobRunId: number;
  jobId: number;
  triggerId: number;
  triggerType: string;
  jobName: string;
  jobTitle: string;
  jobType: string;
  state: string;
  plannedStartUtc: string;
  actualStartUtc?: string;
  actualEndUtc?: string;
  estimatedEndtUtc?: string;
  progress?: number;
  pid?: number;
  instanceParameter?: string | object;
  jobParameter?: string | object;
  resultParameter?: string | object;
  deleted: boolean;
  artefacts?: JobRunArtefactDto[];
  comment?: string;
  userId?: string;
  userDisplayName?: string;
  definition?: string;
}

export interface JobTriggerDtoBase {
  id: number;
  triggerType: string; // Property position is crucial here
  isActive: boolean;
  parameters?: string | object;
  comment?: string;
  userId?: string;
  userDisplayName?: string;
  deleted: boolean;
}

export interface InstantTriggerDto extends JobTriggerDtoBase {
  triggerType: 'Instant';
  delayedMinutes: number;
}

export interface ScheduledTriggerDto extends JobTriggerDtoBase {
  triggerType: 'Scheduled';
  startDateTimeUtc: string;
}

export interface RecurringTriggerDto extends JobTriggerDtoBase {
  triggerType: 'Recurring';
  startDateTimeUtc?: string;
  endDateTimeUtc?: string;
  definition: string;
}

export type JobTriggerDto = InstantTriggerDto | ScheduledTriggerDto | RecurringTriggerDto;

export interface DiskInfoDto {
  name: string;
  freeSpace: number;
  freeSpaceReadable: string;
  totalSpace: number;
  totalSpaceReadable: string;
  freeSpacePercentage: number;
  type: string;
}

export interface MemoryInfoDto {
  totalPhysicalMemory: number;
  freeMemory: number;
  usagePercentage?: number;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
}

export interface DashboardConfig {
  api: string;
  softDeleteJobRunOnRetry: boolean;
}