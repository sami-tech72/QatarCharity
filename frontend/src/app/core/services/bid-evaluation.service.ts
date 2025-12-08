import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { ApiResponse } from '../../shared/models/api-response.model';
import { PagedResult } from '../../shared/models/pagination.model';
import { SupplierBidQueryRequest, SupplierBidSummary } from '../../shared/models/bid-evaluation.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class BidEvaluationService {
  constructor(private readonly api: ApiService) {}

  loadSupplierBids(query: SupplierBidQueryRequest): Observable<PagedResult<SupplierBidSummary>> {
    return this.api
      .get<PagedResult<SupplierBidSummary>>('rfx/bids', {
        params: {
          pageNumber: query.pageNumber,
          pageSize: query.pageSize,
          search: query.search ?? '',
        },
      })
      .pipe(map((response) => this.unwrap(response)));
  }

  reviewBid(
    bidId: string,
    decision: 'approved' | 'rejected' | 'review',
    comments?: string,
  ): Observable<SupplierBidSummary> {
    return this.api
      .post<SupplierBidSummary>(`rfx/bids/${bidId}/review`, {
        decision,
        comments,
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
