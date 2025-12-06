export interface ProcurementRolesResponse {
  mainRole: string;
  subRoles: ProcurementSubRole[];
  defaultPermissions: ProcurementPermission[];
}

export interface CreateProcurementRoleRequest {
  name: string;
  description?: string;
  permissions: ProcurementPermission[];
}

export interface ProcurementSubRole {
  id: number;
  name: string;
  description: string;
  totalUsers: number;
  newUsers: number;
  avatars: string[];
  extraCount?: number;
  permissions: ProcurementPermission[];
}

export interface ProcurementPermission {
  name: string;
  actions: ProcurementPermissionActions;
}

export interface ProcurementPermissionActions {
  read: boolean;
  write: boolean;
  create: boolean;
}
