export type UserRole = 'Admin' | 'Procurement' | 'Supplier';
export type ProcurementSubRole = 'Lead' | 'Sourcing' | 'Reporting';
export type ProcurementPermission =
  | 'dashboard'
  | 'rfx-management'
  | 'rfx-management:create'
  | 'bid-evaluation'
  | 'tender-committee'
  | 'contract-management'
  | 'supplier-performance'
  | 'reports-analytics'
  | 'settings';

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
  procurementSubRole?: ProcurementSubRole | null;
  procurementPermissions?: ProcurementPermission[];
}

export interface UserSession extends LoginResponse {}
