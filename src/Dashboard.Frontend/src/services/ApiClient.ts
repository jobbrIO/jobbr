import axios, { AxiosInstance } from 'axios';
import {
  JobDto,
  JobRunDto,
  JobTriggerDto,
  InstantTriggerDto,
  DiskInfoDto,
  MemoryInfoDto,
  PagedResult,
  DashboardConfig
} from '../types';

class ApiClient {
  private apiClient: AxiosInstance;
  private dashboardClient: AxiosInstance;
  private apiUrl: string = '';
  private softDeleteJobRunOnRetry: boolean = false;
  private initPromise: Promise<void>;

  constructor() {
    // Dashboard client for config, system and cron endpoints
    this.dashboardClient = axios.create({
      baseURL: window.location.origin,
      headers: {
        'Accept': 'application/json',
        'X-Requested-With': 'XMLHttpRequest'
      }
    });

    // Initialize API client after getting config
    this.initPromise = this.initializeApiClient();
    
    // Placeholder API client
    this.apiClient = axios.create();
  }

  private async initializeApiClient(): Promise<void> {
    try {
      const response = await this.dashboardClient.get<DashboardConfig>('/config');
      this.apiUrl = response.data.api;
      this.softDeleteJobRunOnRetry = response.data.softDeleteJobRunOnRetry;

      // Initialize the actual API client
      this.apiClient = axios.create({
        baseURL: this.apiUrl,
        headers: {
          'Accept': 'application/json',
          'X-Requested-With': 'XMLHttpRequest'
        }
      });
    } catch (error) {
      console.error('Failed to initialize API client:', error);
      throw error;
    }
  }

  private async ensureInitialized(): Promise<void> {
    await this.initPromise;
  }

  // System endpoints
  async getCpuInfo(): Promise<number> {
    await this.ensureInitialized();
    const response = await this.dashboardClient.get<number>('/system/cpu');
    return response.data;
  }

  async getMemoryInfo(): Promise<MemoryInfoDto> {
    await this.ensureInitialized();
    const response = await this.dashboardClient.get<MemoryInfoDto>('/system/memory');
    return response.data;
  }

  async getDiskInfo(): Promise<DiskInfoDto[]> {
    await this.ensureInitialized();
    const response = await this.dashboardClient.get<DiskInfoDto[]>('/system/disks');
    return response.data;
  }

  // Job endpoints
  async getAllJobs(): Promise<PagedResult<JobDto>> {
    await this.ensureInitialized();
    const response = await this.apiClient.get<PagedResult<JobDto>>('/jobs?pageSize=10000');
    return response.data;
  }

  async getJob(id: number): Promise<JobDto> {
    await this.ensureInitialized();
    const response = await this.apiClient.get<JobDto>(`/jobs/${id}`);
    return response.data;
  }

  // Job Run endpoints
  async getJobRuns(
    page: number = 1,
    sort: string = '',
    query: string = '',
    states?: string[],
    showDeleted: boolean = false,
    pageSize?: number
  ): Promise<PagedResult<JobRunDto>> {
    await this.ensureInitialized();
    
    let url = `/jobruns/?page=${page}&sort=${sort}&query=${query}&showDeleted=${showDeleted}`;
    
    if (states && states.length > 0) {
      url += `&states=${states.join(',')}`;
    }
    
    if (pageSize) {
      url += `&pageSize=${pageSize}`;
    }
    
    const response = await this.apiClient.get<PagedResult<JobRunDto>>(url);
    return response.data;
  }

  async getJobRunsByJobId(
    jobId: number,
    page: number = 1,
    sort: string = '',
    pageSize?: number,
    showDeleted: boolean = false
  ): Promise<PagedResult<JobRunDto>> {
    await this.ensureInitialized();
    
    let url = `/jobs/${jobId}/runs?page=${page}&showDeleted=${showDeleted}`;
    
    if (sort) {
      url += `&sort=${sort}`;
    }
    
    if (pageSize) {
      url += `&pageSize=${pageSize}`;
    }
    
    const response = await this.apiClient.get<PagedResult<JobRunDto>>(url);
    return response.data;
  }

  async getRunningJobRuns(): Promise<PagedResult<JobRunDto>> {
    await this.ensureInitialized();
    const states = ['Scheduled', 'Preparing', 'Starting', 'Started', 'Connected', 'Initializing', 'Processing', 'Finishing', 'Collecting'];
    const response = await this.apiClient.get<PagedResult<JobRunDto>>(
      `/jobruns/?sort=-PlannedStartDateTimeUtc&pageSize=200&states=${states.join(',')}`
    );
    return response.data;
  }

  async getLastFailedJobRuns(): Promise<PagedResult<JobRunDto>> {
    await this.ensureInitialized();
    const response = await this.apiClient.get<PagedResult<JobRunDto>>(
      '/jobruns/?sort=-ActualEndDateTimeUtc&pageSize=5&states=Failed'
    );
    return response.data;
  }

  async getJobRun(id: number): Promise<JobRunDto> {
    await this.ensureInitialized();
    const response = await this.apiClient.get<JobRunDto>(`/jobruns/${id}`);
    return response.data;
  }

  async deleteJobRun(id: number): Promise<void> {
    await this.ensureInitialized();
    await this.apiClient.delete(`/jobruns/${id}`);
  }

  // Trigger endpoints
  async getTriggersByJobId(jobId: number, page: number = 1): Promise<PagedResult<JobTriggerDto>> {
    await this.ensureInitialized();
    const response = await this.apiClient.get<PagedResult<JobTriggerDto>>(
      `/jobs/${jobId}/triggers/?pageSize=5&page=${page}`
    );
    return response.data;
  }

  async getTrigger(jobId: number, triggerId: number): Promise<JobTriggerDto> {
    await this.ensureInitialized();
    const response = await this.apiClient.get<JobTriggerDto>(`/jobs/${jobId}/triggers/${triggerId}`);
    return response.data;
  }

  async updateTrigger(trigger: JobTriggerDto, jobId: number): Promise<void> {
    await this.ensureInitialized();
    await this.apiClient.patch(`/jobs/${jobId}/triggers/${trigger.id}`, trigger);
  }

  async createTrigger(trigger: Omit<JobTriggerDto, 'id' | 'deleted'>, jobId: number): Promise<void> {
    await this.ensureInitialized();
    await this.apiClient.post(`/jobs/${jobId}/triggers`, trigger);
  }

  // Complex operations
  async retryJobRun(jobRun: JobRunDto): Promise<void> {
    await this.ensureInitialized();
    
    const oldTrigger = await this.getTrigger(jobRun.jobId, jobRun.triggerId);
    
    const newTrigger: Omit<InstantTriggerDto, 'id' | 'deleted'> = {
      triggerType: 'Instant' as const,
      parameters: typeof jobRun.instanceParameter === 'object' 
        ? JSON.stringify(jobRun.instanceParameter)
        : jobRun.instanceParameter || '',
      comment: oldTrigger.comment || '',
      delayedMinutes: 0,
      userId: oldTrigger.userId || '',
      userDisplayName: oldTrigger.userDisplayName || '',
      isActive: true
    };
    
    await this.createTrigger(newTrigger, jobRun.jobId);
    
    if (this.softDeleteJobRunOnRetry) {
      await this.deleteJobRun(jobRun.jobRunId);
    }
  }

  // Cron validation
  async validateCron(cron: string): Promise<boolean> {
    await this.ensureInitialized();
    try {
      const response = await this.dashboardClient.get<{ parseSuccess: boolean }>(
        `/cron/?cron=${encodeURIComponent(cron)}`
      );
      return response.data.parseSuccess;
    } catch {
      return false;
    }
  }

  // Getter for API URL
  async getApiUrl(): Promise<string> {
    await this.ensureInitialized();
    return this.apiUrl;
  }
}

export default ApiClient;