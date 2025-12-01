import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ConfigService } from './config.service';

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(
    private readonly http: HttpClient,
    private readonly config: ConfigService,
  ) {}

  post<T>(endpoint: string, body: unknown, headers?: HttpHeaders): Observable<T> {
    return this.http.post<T>(`${this.config.apiBaseUrl}/${endpoint}`, body, { headers });
  }
}
