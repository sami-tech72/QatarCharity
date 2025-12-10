import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { ApiResponse } from '../../shared/models/api-response.model';
import { ContractManagementQuery, ContractReadyBid } from '../../shared/models/contract-management.model';
import { PagedResult } from '../../shared/models/pagination.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class ContractManagementService {
  constructor(private readonly api: ApiService) {}

  loadReadyBids(query: ContractManagementQuery): Observable<PagedResult<ContractReadyBid>> {
    return this.api
      .get<PagedResult<ContractReadyBid>>('contracts/ready', {
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
