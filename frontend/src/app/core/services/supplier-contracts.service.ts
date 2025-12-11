import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { ApiResponse } from '../../shared/models/api-response.model';
import {
  SignContractPayload,
  SupplierContract,
  SupplierContractQuery,
  SupplierContractResponse,
} from '../../shared/models/supplier-contract.model';
import { ContractDetail } from '../../shared/models/contract-management.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class SupplierContractsService {
  constructor(private readonly api: ApiService) {}

  loadContracts(query: SupplierContractQuery): Observable<SupplierContractResponse> {
    return this.api
      .get<SupplierContractResponse>('supplier/contracts', {
        params: {
          pageNumber: query.pageNumber,
          pageSize: query.pageSize,
          search: query.search ?? '',
        },
      })
      .pipe(map((response) => this.unwrap(response)));
  }

  signContract(contractId: string, payload: SignContractPayload): Observable<SupplierContract> {
    return this.api
      .post<SupplierContract>(`supplier/contracts/${contractId}/sign`, payload)
      .pipe(map((response) => this.unwrap(response)));
  }

  loadContract(contractId: string): Observable<ContractDetail> {
    return this.api.get<ContractDetail>(`supplier/contracts/${contractId}`).pipe(map((response) => this.unwrap(response)));
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data === undefined || response.data === null) {
      throw new Error(response.message || 'Request failed.');
    }

    return response.data;
  }
}
