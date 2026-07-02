export interface PageRequest {
  pageNumber: number;
  pageSize: number;
}

export interface PagedResult<T> {
  items: readonly T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
