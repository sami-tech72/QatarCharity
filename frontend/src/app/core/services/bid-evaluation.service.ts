import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { ApiResponse } from '../../shared/models/api-response.model';
import {
  BidEvaluationQuery,
  EvaluateBidRequest,
  SupplierBidEvaluation,
} from '../../shared/models/bid-evaluation.model';
import { PagedResult } from '../../shared/models/pagination.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class BidEvaluationService {
  constructor(private readonly api: ApiService) {}

  loadBids(query: BidEvaluationQuery): Observable<PagedResult<SupplierBidEvaluation>> {
    return this.api
      .get<PagedResult<SupplierBidEvaluation>>('rfx/bids', {
        params: {
          pageNumber: query.pageNumber,
          pageSize: query.pageSize,
          search: query.search ?? '',
        },
      })
      .pipe(map((response) => this.unwrap(response)));
  }

  evaluateBid(rfxId: string, bidId: string, request: EvaluateBidRequest): Observable<SupplierBidEvaluation> {
    return this.api
      .post<SupplierBidEvaluation>(`rfx/${rfxId}/bids/${bidId}/evaluate`, request)
      .pipe(map((response) => this.unwrap(response)));
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data === undefined || response.data === null) {
      throw new Error(response.message || 'Request failed.');
    }

    return response.data;
  }
}
