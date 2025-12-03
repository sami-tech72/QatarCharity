import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { ApiResponse } from '../../shared/models/api-response.model';
import { PagedResult } from '../../shared/models/pagination.model';
import { Supplier, SupplierQueryRequest, SupplierRequest } from '../../shared/models/supplier.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class SupplierManagementService {
  constructor(private readonly api: ApiService) {}

  loadSuppliers(query: SupplierQueryRequest): Observable<PagedResult<Supplier>> {
    return this.api
      .get<PagedResult<Supplier>>('suppliers', {
        params: {
          pageNumber: query.pageNumber,
          pageSize: query.pageSize,
          search: query.search ?? '',
        },
      })
      .pipe(map((response) => this.unwrap(response)));
  }

  createSupplier(request: SupplierRequest): Observable<Supplier> {
    return this.api.post<Supplier>('suppliers', request).pipe(map((response) => this.unwrap(response)));
  }

  updateSupplier(id: string, request: SupplierRequest): Observable<Supplier> {
    return this.api.put<Supplier>(`suppliers/${id}`, request).pipe(map((response) => this.unwrap(response)));
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data === undefined || response.data === null) {
      throw new Error(response.message || 'Request failed.');
    }

    return response.data;
  }
}
