export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errorCode?: string;
  details?: Record<string, unknown>;
  timestamp: string;
}
