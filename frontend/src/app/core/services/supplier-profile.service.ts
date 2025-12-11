import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { SupplierProfile } from '../../shared/models/supplier-profile.model';
import { Supplier } from '../../shared/models/supplier.model';
import { ApiResponse } from '../../shared/models/api-response.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class SupplierProfileService {
  constructor(private readonly api: ApiService) {}

  loadProfile(): Observable<Supplier> {
    return this.api.get<Supplier>('supplier/profile').pipe(map((response) => this.unwrap(response)));
  }

  updateProfile(request: SupplierProfile): Observable<Supplier> {
    return this.api.put<Supplier>('supplier/profile', request).pipe(map((response) => this.unwrap(response)));
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data === undefined || response.data === null) {
      throw new Error(response.message || 'Request failed.');
    }

    return response.data;
  }
}
