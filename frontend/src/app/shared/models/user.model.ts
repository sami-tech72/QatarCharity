export type UserRole = 'Admin' | 'Procurement' | 'Supplier';
export type ProcurementSubRole =
  | 'ProcurementManager'
  | 'ProcurementOfficer'
  | 'ProcurementViewer';

export const PROCUREMENT_SUB_ROLES: ProcurementSubRole[] = [
  'ProcurementManager',
  'ProcurementOfficer',
  'ProcurementViewer',
];

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  email: string;
  displayName: string;
  role: UserRole;
  roles: string[];
  subRoles: ProcurementSubRole[];
  token: string;
  expiresAt: string;
}

export interface UserSession extends LoginResponse {}
