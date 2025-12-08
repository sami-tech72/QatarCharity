export interface SupplierBidQueryRequest {
  pageNumber: number;
  pageSize: number;
  search?: string;
}

export interface SupplierBidSummary {
  id: string;
  rfxId: string;
  rfxReferenceNumber: string;
  rfxTitle: string;
  supplierName: string;
  bidAmount: number;
  currency: string;
  expectedDeliveryDate?: string | null;
  submittedAtUtc: string;
  proposalSummary: string;
  notes?: string | null;
}
