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
  submittedByUserId: string;
  submittedByName: string;
  bidAmount: number;
  currency: string;
  expectedDeliveryDate?: string | null;
  submittedAtUtc: string;
  proposalSummary: string;
  notes?: string | null;
  status?: string;
  reviews?: BidReviewSummary[];
}

export interface BidReviewSummary {
  id: string;
  bidId: string;
  reviewerUserId: string;
  reviewerName: string;
  decision: string;
  reviewedAtUtc: string;
  comments?: string | null;
}
