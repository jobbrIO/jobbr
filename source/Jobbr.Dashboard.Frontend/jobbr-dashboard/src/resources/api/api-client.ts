import { DashboardMemoryResponse, DiskInfoDto, JobDetailsDto, JobDto, JobRunDto, JobTriggerDto } from './dtos';
import { autoinject } from "aurelia-framework";
import { HttpClient } from 'aurelia-fetch-client';
import { PagedResult } from './paged-result';

@autoinject
export class ApiClient {

  private httpClient: HttpClient;

  constructor() {
    this.httpClient = new HttpClient();
      
    this.httpClient.configure(config => {
      config
        .useStandardConfiguration()
        .withDefaults({
          credentials: 'same-origin',
          headers: {
            'Accept': 'application/json',
            'X-Requested-With': 'Fetch'
          }
        })
        .withBaseUrl('http://localhost:1338');
    });
  }

  getCpuInfo(): Promise<number> {
    return this.httpClient.fetch('/system/cpu').then(r => r.json()).catch(e => console.log(e));
  }

  getMemoryInfo(): Promise<DashboardMemoryResponse> {
    return this.httpClient.fetch('/system/memory').then(r => r.json());
  }

  getDiskInfo(): Promise<Array<DiskInfoDto>> {
    return this.httpClient.fetch('/system/disks').then(r => r.json());
  }

  getAllJobs(): Promise<PagedResult<JobDto>> {
    return this.httpClient.fetch('/api/jobs').then(r => r.json());
  }

  getJob(id: number): Promise<JobDto> {
    return this.httpClient.fetch('/api/jobs/' + id).then(r => r.json());
  }

  getJobRunsByJobId(jobId: number, page: number = 1, sort: string = '', states: Array<string> = null): Promise<PagedResult<JobRunDto>> {
    let url = '/api/jobs/' + jobId + '/runs?page=' + page + '&sort=' + sort;

    // todo: states not yet supported here by webapi
    if (states) {
      url = url += '&states=' + states.join(',');
    }

    return this.httpClient.fetch(url).then(r => r.json());
  }

  getJobRuns(page: number = 1, sort: string = '', query: string = '', states: Array<string> = null): Promise<PagedResult<JobRunDto>> {
    let url = '/api/jobruns/?page=' + page + '&sort=' + sort + '&query=' + query;

    if (states) {
      url = url += '&states=' + states.join(',');
    }

    return this.httpClient.fetch(url).then(r => r.json());
  }

  getLastFailedJobRuns() {
    return this.httpClient.fetch('/api/jobruns/?sort=-ActualEndDateTimeUtc&pageSize=5&states=Failed').then(r => r.json());
  }
  
  getJobRun(id: number): Promise<JobRunDto> {
    return this.httpClient.fetch('/api/jobruns/' + id).then(r => r.json());
  }

  getTrigger(jobId: number, triggerId: number): Promise<JobTriggerDto> {
    return this.httpClient.fetch('/api/jobs/' + jobId + '/triggers/' + triggerId).then(r => r.json());
  }

  getJobDetails(jobId: number): Promise<JobDetailsDto> {
    return this.httpClient.fetch('/api/jobs/' + jobId).then(r => r.json());
  }

  getRunningJobRuns(): Promise<PagedResult<JobRunDto>> {
    return this.httpClient.fetch('/api/jobruns/?sort=-PlannedStartDateTimeUtc&pageSize=200&states=Scheduled,Preparing,Starting,Started,Connected,Initializing,Processing,Finishing,Collecting').then(r => r.json());
  }
}
