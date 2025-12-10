import { PagedResult } from './pagination.model';

export interface SupplierContract {
  id: string;
  bidId?: string | null;
  rfxId?: string | null;
  referenceNumber: string;
  title: string;
  supplierName: string;
  supplierUserId: string;
  contractValue: number;
  currency: string;
  startDateUtc: string;
  endDateUtc: string;
  status: string;
  createdAtUtc: string;
  supplierSignature?: string | null;
  supplierSignedAtUtc?: string | null;
}

export interface SupplierContractQuery {
  pageNumber: number;
  pageSize: number;
  search?: string;
}

export interface SupplierContractResponse extends PagedResult<SupplierContract> {}

export interface SignContractPayload {
  signature: string;
}
