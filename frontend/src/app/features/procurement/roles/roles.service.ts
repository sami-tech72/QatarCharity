import { Injectable } from '@angular/core';
import { Observable, catchError, map, of } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { ApiResponse } from '../../../shared/models/api-response.model';
import { ProcurementRolePayload } from './models/procurement-role.model';

@Injectable({ providedIn: 'root' })
export class RolesService {
  constructor(private readonly api: ApiService) {}

  getProcurementRoles(): Observable<ProcurementRolePayload> {
    return this.api.get<ProcurementRolePayload>('procurement/roles').pipe(
      map((response: ApiResponse<ProcurementRolePayload>) => response.data ?? this.getFallbackPayload()),
      catchError(() => of(this.getFallbackPayload())),
    );
  }

  private getFallbackPayload(): ProcurementRolePayload {
    const menuPermissions = [
      { menu: 'Supplier Management', view: true, edit: true, create: true, delete: true },
      { menu: 'RFx Management', view: true, edit: true, create: true, delete: false },
      { menu: 'Purchase Orders', view: true, edit: true, create: true, delete: false },
      { menu: 'Contracts', view: true, edit: true, create: false, delete: false },
      { menu: 'Invoices', view: true, edit: false, create: false, delete: false },
      { menu: 'Reports', view: true, edit: false, create: false, delete: false },
      { menu: 'Settings', view: true, edit: true, create: false, delete: false },
    ];

    return {
      mainRole: 'Procurement',
      subRoles: [
        {
          name: 'Procurement Admin',
          users: 6,
          avatars: ['AN', 'MT', 'CR', 'HD'],
          extraUsers: 2,
          badge: 'Default',
          permissions: menuPermissions,
        },
        {
          name: 'Category Manager',
          users: 5,
          avatars: ['LS', 'BK', 'AO', 'TT'],
          extraUsers: 1,
          permissions: menuPermissions.map((p) => ({ ...p, delete: false })),
        },
        {
          name: 'Sourcing Specialist',
          users: 4,
          avatars: ['GM', 'ID', 'RS', 'LP'],
          permissions: menuPermissions.map((p) => ({ ...p, edit: p.menu !== 'Settings', delete: false })),
        },
        {
          name: 'Requester',
          users: 3,
          avatars: ['CF', 'NI', 'JD'],
          permissions: menuPermissions.map((p) => ({
            ...p,
            edit: false,
            create: p.menu === 'RFx Management' || p.menu === 'Purchase Orders',
            delete: false,
          })),
        },
      ],
      menuPermissions,
    };
  }
}
