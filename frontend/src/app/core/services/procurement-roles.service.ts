import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { ApiService } from './api.service';
import { ApiResponse } from '../../shared/models/api-response.model';
import {
  CreateProcurementRoleRequest,
  ProcurementRolesResponse,
  ProcurementSubRole,
  UpdateProcurementRoleRequest,
} from '../../shared/models/procurement-roles.model';

@Injectable({ providedIn: 'root' })
export class ProcurementRolesService {
  constructor(private readonly api: ApiService) {}

  loadRoles(): Observable<ProcurementRolesResponse> {
    return this.api.get<ProcurementRolesResponse>('procurement/roles').pipe(map((response) => this.unwrap(response)));
  }

  createRole(payload: CreateProcurementRoleRequest): Observable<ProcurementSubRole> {
    return this.api.post<ProcurementSubRole>('procurement/roles', payload).pipe(map((response) => this.unwrap(response)));
  }

  updateRole(id: number, payload: UpdateProcurementRoleRequest): Observable<ProcurementSubRole> {
    return this.api
      .put<ProcurementSubRole>(`procurement/roles/${id}`, payload)
      .pipe(map((response) => this.unwrap(response)));
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data === undefined || response.data === null) {
      throw new Error(response.message || 'Request failed.');
    }

    return response.data;
  }
}
