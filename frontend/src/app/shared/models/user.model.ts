export type UserRole = 'Admin' | 'Procurement' | 'Supplier';
export type ProcurementPermission =
  | 'Procurement.View'
  | 'Procurement.Create'
  | 'Procurement.Update'
  | 'Procurement.Delete';

export interface ProcurementAccess {
  subRoles: string[];
  permissions: ProcurementPermission[];
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
  procurementAccess?: ProcurementAccess;
}

export interface UserSession extends LoginResponse {}
