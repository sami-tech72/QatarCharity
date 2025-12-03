import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { ApiResponse } from '../../shared/models/api-response.model';
import { PagedResult } from '../../shared/models/pagination.model';
import {
  CreateUserRequest,
  ManagedUser,
  UpdateUserRequest,
  UserQueryRequest,
} from '../../shared/models/user-management.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class UserManagementService {
  constructor(private readonly api: ApiService) {}

  loadUsers(query: UserQueryRequest): Observable<PagedResult<ManagedUser>> {
    return this.api
      .get<PagedResult<ManagedUser>>('users', {
        params: {
          pageNumber: query.pageNumber,
          pageSize: query.pageSize,
          search: query.search ?? '',
        },
      })
      .pipe(map((response) => this.unwrap(response)));
  }

  createUser(request: CreateUserRequest): Observable<ManagedUser> {
    return this.api.post<ManagedUser>('users', request).pipe(map((response) => this.unwrap(response)));
  }

  updateUser(id: string, request: UpdateUserRequest): Observable<ManagedUser> {
    return this.api.put<ManagedUser>(`users/${id}`, request).pipe(map((response) => this.unwrap(response)));
  }

  deleteUser(id: string): Observable<void> {
    return this.api.delete<null>(`users/${id}`).pipe(
      map((response) => {
        if (!response.success) {
          throw new Error(response.message || 'Request failed.');
        }

        return;
      }),
    );
  }

  getUserLookup(search?: string): Observable<ManagedUser[]> {
    return this.api
      .get<ManagedUser[]>(`users/lookup`, { params: { search: search ?? '' } })
      .pipe(map((response) => this.unwrap(response)));
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data === undefined || response.data === null) {
      throw new Error(response.message || 'Request failed.');
    }

    return response.data;
  }
}
