export type UserRole = 'Admin' | 'Procurement' | 'Supplier';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  email: string;
  displayName: string;
  role: UserRole;
  roles: string[];
  subRoles: string[];
  token: string;
  expiresAt: string;
}

export interface UserSession extends LoginResponse {}
