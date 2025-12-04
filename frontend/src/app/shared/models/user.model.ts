export type UserRole = 'Admin' | 'Procurement' | 'CommitteeMember' | 'Supplier';

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
}

export interface UserSession extends LoginResponse {}
