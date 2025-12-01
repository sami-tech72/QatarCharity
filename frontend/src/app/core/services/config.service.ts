import { Injectable } from '@angular/core';
import { environmentConfig } from '../config/environment.config';

@Injectable({ providedIn: 'root' })
export class ConfigService {
  readonly apiBaseUrl = environmentConfig.apiBaseUrl;
}
