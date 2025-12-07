export interface SupplierRfxQueryRequest {
  pageNumber: number;
  pageSize: number;
  search?: string;
}

export interface SupplierBidRequest {
  bidAmount: number | null;
  currency: string;
  expectedDeliveryDate?: string;
  proposalSummary: string;
  notes?: string;
}

export interface SupplierRfx {
  id: string;
  referenceNumber: string;
  rfxType: string;
  title: string;
  category: string;
  description: string;
  publicationDate: string;
  submissionDeadline: string;
  closingDate: string;
  estimatedBudget: number;
  currency: string;
  hideBudget: boolean;
}
