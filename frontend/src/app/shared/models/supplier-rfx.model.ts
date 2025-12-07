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
  documents: BidDocumentSubmission[];
  inputs: BidInputSubmission[];
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
  scope: string;
  technicalSpecification: string;
  deliverables: string;
  timeline: string;
  requiredDocuments: string[];
  requiredDetails: string[];
  requiredInputs: string[];
}

export interface BidDocumentSubmission {
  name: string;
  value: string;
}

export interface BidInputSubmission {
  name: string;
  value: string;
}
