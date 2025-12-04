import { PageRequest } from './pagination.model';
import { ProcurementSubRole, UserRole } from './user.model';

export interface ManagedUser {
  id: string;
  displayName: string;
  email: string;
  role: UserRole;
  subRoles: ProcurementSubRole[];
}

export interface CreateUserRequest {
  displayName: string;
  email: string;
  password: string;
  role: UserRole;
  subRoles: ProcurementSubRole[];
}

export interface UpdateUserRequest {
  displayName: string;
  email: string;
  role: UserRole;
  subRoles: ProcurementSubRole[];
}

export interface UserQueryRequest extends PageRequest {}
