export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface PageRequest {
  pageNumber: number;
  pageSize: number;
  search?: string;
}
