import { ApiClient } from '../resources/api/api-client';
import { JobDto } from '../resources/api/dtos';
import { autoinject } from 'aurelia-dependency-injection';
import * as $ from 'jquery';

@autoinject
export class Jobs {
  public jobs: Array<JobDto>;

  constructor(private api: ApiClient, private element: Element) {
    this.jobs = [];
  }

  public attached() {
    this.api.getAllJobs().then(r => this.jobs = r);
  }
}
