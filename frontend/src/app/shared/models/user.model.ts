import { ProcurementPermission } from './procurement-roles.model';

export type UserRole = 'Admin' | 'Procurement' | 'Supplier';

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
  procurementRole?: ProcurementUserRole | null;
}

export interface UserSession extends LoginResponse {}

export interface ProcurementUserRole {
  id: number;
  name: string;
  permissions: ProcurementPermission[];
}
