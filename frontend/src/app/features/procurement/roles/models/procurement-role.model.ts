export interface ProcurementPermission {
  menu: string;
  view: boolean;
  edit: boolean;
  create: boolean;
  delete: boolean;
}

export interface ProcurementSubRole {
  name: string;
  users: number;
  avatars: string[];
  extraUsers?: number;
  badge?: string;
  permissions: ProcurementPermission[];
}

export interface ProcurementRolePayload {
  mainRole: string;
  subRoles: ProcurementSubRole[];
  menuPermissions: ProcurementPermission[];
}
