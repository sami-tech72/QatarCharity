export type UserRole = 'Admin' | 'Procurement' | 'Supplier';
export type ProcurementSubRole =
  | 'ProcurementViewer'
  | 'ProcurementContributor'
  | 'ProcurementManager';

export interface ProcurementPermissionSet {
  canView: boolean;
  canCreate: boolean;
  canUpdate: boolean;
  canDelete: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  email: string;
  displayName: string;
  role: UserRole;
  token: string;
  expiresAt: string;
  procurementSubRoles?: ProcurementSubRole[];
  procurementPermissions?: ProcurementPermissionSet;
}

export interface UserSession extends LoginResponse {}
