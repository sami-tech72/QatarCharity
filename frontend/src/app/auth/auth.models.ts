import { SidebarRole } from '../components/sidebar/sidebar.component';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  email: string;
  displayName: string;
  role: SidebarRole;
  token: string;
  expiresAt: string;
}

export interface UserSession extends LoginResponse {}
