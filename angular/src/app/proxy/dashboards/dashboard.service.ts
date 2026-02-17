import type { DashboardDto, DashboardInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  apiName = 'Default';
  

  get = (input: DashboardInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardDto>({
      method: 'POST',
      url: '/api/app/dashboard',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
