import { PageRequest } from './pagination.model';
import { UserRole } from './user.model';

export interface ManagedUser {
  id: string;
  displayName: string;
  email: string;
  role: UserRole;
}

export interface CreateUserRequest {
  displayName: string;
  email: string;
  password: string;
  role: UserRole;
}

export interface UpdateUserRequest {
  displayName: string;
  email: string;
  role: UserRole;
}

export interface UserQueryRequest extends PageRequest {}
