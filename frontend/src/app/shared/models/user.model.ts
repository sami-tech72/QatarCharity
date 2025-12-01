export type UserRole = 'Admin' | 'Procurement' | 'Supplier';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  role: UserRole;
  username?: string;
  expiresAt?: string;
}

export interface UserSession extends LoginResponse {}
