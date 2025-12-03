import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { ApiResponse } from '../../shared/models/api-response.model';
import { PagedResult } from '../../shared/models/pagination.model';
import {
  UpsertWorkflowRequest,
  WorkflowDetail,
  WorkflowQueryRequest,
  WorkflowSummary,
} from '../../shared/models/workflow.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class WorkflowService {
  constructor(private readonly api: ApiService) {}

  loadWorkflows(query: WorkflowQueryRequest): Observable<PagedResult<WorkflowSummary>> {
    return this.api
      .get<PagedResult<WorkflowSummary>>('workflows', {
        params: {
          pageNumber: query.pageNumber,
          pageSize: query.pageSize,
          search: query.search ?? '',
        },
      })
      .pipe(map((response) => this.unwrap(response)));
  }

  getWorkflow(id: string): Observable<WorkflowDetail> {
    return this.api
      .get<WorkflowDetail>(`workflows/${id}`)
      .pipe(map((response) => this.unwrap(response)));
  }

  createWorkflow(request: UpsertWorkflowRequest): Observable<WorkflowDetail> {
    return this.api
      .post<WorkflowDetail>('workflows', request)
      .pipe(map((response) => this.unwrap(response)));
  }

  updateWorkflow(id: string, request: UpsertWorkflowRequest): Observable<WorkflowDetail> {
    return this.api
      .put<WorkflowDetail>(`workflows/${id}`, request)
      .pipe(map((response) => this.unwrap(response)));
  }

  deleteWorkflow(id: string): Observable<void> {
    return this.api.delete<null>(`workflows/${id}`).pipe(
      map((response) => {
        if (!response.success) {
          throw new Error(response.message || 'Request failed.');
        }
        return;
      }),
    );
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data === undefined || response.data === null) {
      throw new Error(response.message || 'Request failed.');
    }

    return response.data;
  }
}
