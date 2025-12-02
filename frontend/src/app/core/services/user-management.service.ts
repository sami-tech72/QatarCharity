import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { ApiResponse } from '../../shared/models/api-response.model';
import { CreateUserRequest, ManagedUser } from '../../shared/models/user-management.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class UserManagementService {
  constructor(private readonly api: ApiService) {}

  loadUsers(): Observable<ManagedUser[]> {
    return this.api.get<ManagedUser[]>('users').pipe(map((response) => this.unwrap(response)));
  }

  createUser(request: CreateUserRequest): Observable<ManagedUser> {
    return this.api.post<ManagedUser>('users', request).pipe(map((response) => this.unwrap(response)));
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data === undefined || response.data === null) {
      throw new Error(response.message || 'Request failed.');
    }

    return response.data;
  }
}
