export interface ContractReadyBid {
  bidId: string;
  rfxId: string;
  referenceNumber: string;
  title: string;
  supplierUserId: string;
  supplierName: string;
  bidAmount: number;
  currency: string;
  evaluationStatus: string;
  submittedAtUtc: string;
  evaluatedAtUtc?: string | null;
}

export interface ContractManagementQuery {
  pageNumber: number;
  pageSize: number;
  search?: string;
}

export interface CreateContractPayload {
  bidId?: string | null;
  rfxId?: string | null;
  directContract?: boolean;
  title: string;
  supplierName: string;
  supplierUserId: string;
  contractValue: number;
  currency: string;
  startDateUtc: string;
  endDateUtc: string;
}

export interface ContractResponse {
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

export type ContractRecord = ContractResponse;

export interface ContractCompanyDetails {
  name: string;
  address: string;
  contactEmail: string;
  contactPhone: string;
}

export interface ContractSupplierDetails {
  companyName: string;
  contactName: string;
  contactEmail: string;
  contactPhone: string;
  companyAddress: string;
  supplierName: string;
  supplierUserId: string;
}

export interface ContractDetail extends ContractResponse {
  issuerCompany: ContractCompanyDetails;
  supplier: ContractSupplierDetails;
}
