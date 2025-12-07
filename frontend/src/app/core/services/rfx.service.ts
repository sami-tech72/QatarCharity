import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { ApiResponse } from '../../shared/models/api-response.model';
import { PagedResult } from '../../shared/models/pagination.model';
import { CreateRfxRequest, RfxDetail, RfxQueryRequest, RfxSummary } from '../../shared/models/rfx.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class RfxService {
  constructor(private readonly api: ApiService) {}

  loadRfx(query: RfxQueryRequest): Observable<PagedResult<RfxSummary>> {
    return this.api
      .get<PagedResult<RfxSummary>>('rfx', {
        params: {
          pageNumber: query.pageNumber,
          pageSize: query.pageSize,
          search: query.search ?? '',
        },
      })
      .pipe(map((response) => this.unwrap(response)));
  }

  createRfx(request: CreateRfxRequest): Observable<RfxDetail> {
    return this.api.post<RfxDetail>('rfx', request).pipe(map((response) => this.unwrap(response)));
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data === undefined || response.data === null) {
      throw new Error(response.message || 'Request failed.');
    }

    return response.data;
  }
}
