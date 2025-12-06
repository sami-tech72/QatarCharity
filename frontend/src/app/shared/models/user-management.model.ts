import { PageRequest } from './pagination.model';
import { ProcurementUserRole, UserRole } from './user.model';

export interface ManagedUser {
  id: string;
  displayName: string;
  email: string;
  role: UserRole;
  procurementRole?: ProcurementUserRole | null;
}

export interface CreateUserRequest {
  displayName: string;
  email: string;
  password: string;
  role: UserRole;
  procurementRoleTemplateId?: number | null;
}

export interface UpdateUserRequest {
  displayName: string;
  email: string;
  role: UserRole;
  procurementRoleTemplateId?: number | null;
}

export interface UserQueryRequest extends PageRequest {}
