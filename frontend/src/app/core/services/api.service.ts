import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ConfigService } from './config.service';
import { ApiResponse } from '../../shared/models/api-response.model';

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(
    private readonly http: HttpClient,
    private readonly config: ConfigService,
  ) {}

  get<T>(
    endpoint: string,
    options?: {
      headers?: HttpHeaders;
      params?: HttpParams | Record<string, string | number | boolean | readonly (string | number | boolean)[]>;
    },
  ): Observable<ApiResponse<T>> {
    const { headers, params } = options || {};

    return this.http.get<ApiResponse<T>>(`${this.config.apiBaseUrl}/${endpoint}`, {
      headers,
      params: params ?? {},
      responseType: 'json',
    });
  }

  post<T>(endpoint: string, body: unknown, headers?: HttpHeaders): Observable<ApiResponse<T>> {
    return this.http.post<ApiResponse<T>>(`${this.config.apiBaseUrl}/${endpoint}`, body, { headers });
  }

  put<T>(endpoint: string, body: unknown, headers?: HttpHeaders): Observable<ApiResponse<T>> {
    return this.http.put<ApiResponse<T>>(`${this.config.apiBaseUrl}/${endpoint}`, body, { headers });
  }

  delete<T>(endpoint: string, headers?: HttpHeaders): Observable<ApiResponse<T>> {
    return this.http.delete<ApiResponse<T>>(`${this.config.apiBaseUrl}/${endpoint}`, { headers });
  }
}
