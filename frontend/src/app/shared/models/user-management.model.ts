import { PageRequest } from './pagination.model';
import { ProcurementSubRole, UserRole } from './user.model';

export interface ManagedUser {
  id: string;
  displayName: string;
  email: string;
  role: UserRole;
  procurementSubRoles: ProcurementSubRole[];
}

export interface CreateUserRequest {
  displayName: string;
  email: string;
  password: string;
  role: UserRole;
  procurementSubRoles?: ProcurementSubRole[];
}

export interface UpdateUserRequest {
  displayName: string;
  email: string;
  role: UserRole;
  procurementSubRoles?: ProcurementSubRole[];
}

export interface UserQueryRequest extends PageRequest {}
