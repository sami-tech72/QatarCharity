import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { ApiResponse } from '../../shared/models/api-response.model';
import { PagedResult } from '../../shared/models/pagination.model';
import {
  SupplierBidRequest,
  SupplierBidQueryRequest,
  SupplierBidSummary,
  SupplierRfx,
  SupplierRfxQueryRequest,
} from '../../shared/models/supplier-rfx.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class SupplierRfxService {
  constructor(private readonly api: ApiService) {}

  loadPublishedRfx(query: SupplierRfxQueryRequest): Observable<PagedResult<SupplierRfx>> {
    return this.api
      .get<PagedResult<SupplierRfx>>('supplier/rfx/published', {
        params: {
          pageNumber: query.pageNumber,
          pageSize: query.pageSize,
          search: query.search ?? '',
        },
      })
      .pipe(map((response) => this.unwrap(response)));
  }

  getPublishedRfxById(rfxId: string): Observable<SupplierRfx> {
    return this.api
      .get<SupplierRfx>(`supplier/rfx/published/${rfxId}`)
      .pipe(map((response) => this.unwrap(response)));
  }

  submitBid(rfxId: string, payload: SupplierBidRequest): Observable<string> {
    return this.api
      .post<string>(`supplier/rfx/${rfxId}/bid`, payload)
      .pipe(map((response) => this.unwrap(response)));
  }

  loadSupplierBids(query: SupplierBidQueryRequest): Observable<PagedResult<SupplierBidSummary>> {
    return this.api
      .get<PagedResult<SupplierBidSummary>>('supplier/rfx/bids', {
        params: {
          pageNumber: query.pageNumber,
          pageSize: query.pageSize,
          search: query.search ?? '',
        },
      })
      .pipe(map((response) => this.unwrap(response)));
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data === undefined || response.data === null) {
      throw new Error(response.message || 'Request failed.');
    }

    return response.data;
  }
}
