import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { ApiService } from './api.service';
import { ApiResponse } from '../../shared/models/api-response.model';
import { ProcurementRolesResponse } from '../../shared/models/procurement-roles.model';

@Injectable({ providedIn: 'root' })
export class ProcurementRolesService {
  constructor(private readonly api: ApiService) {}

  loadRoles(): Observable<ProcurementRolesResponse> {
    return this.api.get<ProcurementRolesResponse>('procurement/roles').pipe(map((response) => this.unwrap(response)));
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data === undefined || response.data === null) {
      throw new Error(response.message || 'Request failed.');
    }

    return response.data;
  }
}
