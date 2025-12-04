import { PageRequest } from './pagination.model';
import { ProcurementSubRole, UserRole } from './user.model';

export interface ManagedUser {
  id: string;
  displayName: string;
  email: string;
  role: UserRole;
  procurementSubRole?: ProcurementSubRole;
  procurementCanCreate: boolean;
  procurementCanDelete: boolean;
  procurementCanView: boolean;
  procurementCanEdit: boolean;
}

export interface CreateUserRequest {
  displayName: string;
  email: string;
  password: string;
  role: UserRole;
  procurementSubRole?: ProcurementSubRole;
  procurementCanCreate: boolean;
  procurementCanDelete: boolean;
  procurementCanView: boolean;
  procurementCanEdit: boolean;
}

export interface UpdateUserRequest {
  displayName: string;
  email: string;
  role: UserRole;
  procurementSubRole?: ProcurementSubRole;
  procurementCanCreate: boolean;
  procurementCanDelete: boolean;
  procurementCanView: boolean;
  procurementCanEdit: boolean;
}

export interface UserQueryRequest extends PageRequest {}
