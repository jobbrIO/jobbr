export class JobDto {
  id: number;
  uniqueName: string;
  type: string;
  parameters: any;
}

export class DashboardMemoryResponse {
  totalPhysicalMemory: number;
  freeMemory: number
}

export class DiskInfoDto {
  name: string;
  freeSpace: number;
  totalSPace: number;
  freeSpacePercentage: number;
  type: string;
}

export class JobRunDto {
  jobRunId: number;
  jobId: number;
  triggerId: number;
  jobParameter: string;
  instanceParameter: string;
  jobName: string;
  state: string;
  progress: number;
  plannedStartUtc: string;
  actualStartUtc: string;
  estimatedEndUtc: string;
  actualEndUtc: string;
  artefacts: Array<JobRunArtefactDto>;
  jobTitle: string;
}

export class JobRunArtefactDto {
  filename: string;
  size: number;
  contentType: string
}

export class JobTriggerDto {
  id: number;
  triggerType: string;
  isActive: boolean;
  parameters: string;
  comment: string;
  userId: string;
  userDisplayName: string;
  startDateTimeUtc: string;
  endDateTimeUtc: string;
  definition: string;
  delayedMinutes: number;
}

export class JobDetailsDto {
  id: number;
  uniqueName: string;
  type: string;
  parameters: string;
  trigger: Array<JobTriggerDto>;
}
