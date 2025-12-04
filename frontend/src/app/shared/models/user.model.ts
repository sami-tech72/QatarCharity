export type UserRole = 'Admin' | 'Procurement' | 'Supplier';
// The backend accepts any procurement sub-role name via claims, so keep the
// type open-ended to avoid losing dynamically assigned roles.
export type ProcurementSubRole = string;

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
