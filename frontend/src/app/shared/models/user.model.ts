export type UserRole = 'Admin' | 'Procurement' | 'Supplier';

export type ProcurementSubRole =
  | 'Dashboard'
  | 'RFx Management'
  | 'Bid Evaluation'
  | 'Tender Committee'
  | 'Contract Management'
  | 'Supplier Performance'
  | 'Reports & Analytics'
  | 'Settings';

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
