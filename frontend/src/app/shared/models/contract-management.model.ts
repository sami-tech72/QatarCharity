export interface ContractReadyBid {
  bidId: string;
  rfxId: string;
  referenceNumber: string;
  title: string;
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
