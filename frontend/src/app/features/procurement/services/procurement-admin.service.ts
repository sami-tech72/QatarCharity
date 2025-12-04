import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { ApiResponse } from '../../../shared/models/api-response.model';
import { ProcurementPermissionSet } from '../../../shared/models/user.model';

export interface AssignProcurementSubRoleRequest {
  name: string;
  canView: boolean;
  canCreate: boolean;
  canUpdate: boolean;
  canDelete: boolean;
}

export interface ProcurementSubRoleGrant {
  name: string;
  permissions: ProcurementPermissionSet;
}

export interface ProcurementSubRoleUpdateResult {
  userId: string;
  subRoles: ProcurementSubRoleGrant[];
  permissions: ProcurementPermissionSet;
}

@Injectable({ providedIn: 'root' })
export class ProcurementAdminService {
  constructor(private readonly api: ApiService) {}

  assignSubRole(
    userId: string,
    payload: AssignProcurementSubRoleRequest,
  ): Observable<ApiResponse<ProcurementSubRoleUpdateResult>> {
    return this.api.post<ProcurementSubRoleUpdateResult>(
      `procurement/users/${userId}/sub-roles`,
      payload,
    );
  }
}
