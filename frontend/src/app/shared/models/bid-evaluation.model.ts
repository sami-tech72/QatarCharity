export interface BidEvaluationQuery {
  pageNumber: number;
  pageSize: number;
  search?: string;
}

export interface SupplierBidEvaluation {
  id: string;
  rfxId: string;
  referenceNumber: string;
  title: string;
  submittedBy: string;
  bidAmount: number;
  currency: string;
  expectedDeliveryDate?: string | null;
  proposalSummary: string;
  notes?: string | null;
  submittedAtUtc: string;
  evaluationStatus: string;
  evaluationNotes?: string | null;
  evaluatedAtUtc?: string | null;
  evaluatedBy?: string | null;
  reviews: BidReview[];
}

export interface EvaluateBidRequest {
  status: string;
  reviewNotes?: string;
}

export interface BidReview {
  reviewerName: string;
  status: string;
  notes?: string | null;
  reviewedAtUtc: string;
}
