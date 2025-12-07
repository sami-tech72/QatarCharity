export interface RfxQueryRequest {
  pageNumber: number;
  pageSize: number;
  search?: string;
  assignedOnly?: boolean;
}

export interface RfxSummary {
  id: string;
  referenceNumber: string;
  title: string;
  category: string;
  status: string;
  committeeStatus: string;
  canApprove: boolean;
  closingDate: string;
  estimatedBudget: number;
  currency: string;
  workflowName?: string | null;
}

export interface RfxDetail extends RfxSummary {
  rfxType: string;
  department: string;
  description: string;
  hideBudget: boolean;
  publicationDate: string;
  submissionDeadline: string;
  priority: string;
  tenderBondRequired: boolean;
  contactPerson: string;
  contactEmail: string;
  contactPhone: string;
  scope: string;
  technicalSpecification: string;
  deliverables: string;
  timeline: string;
  requiredDocuments: string[];
  minimumScore: number;
  evaluationNotes?: string | null;
  workflowId?: string | null;
  evaluationCriteria: RfxCriterion[];
  committeeMembers: RfxCommitteeMember[];
  createdAt: string;
  lastModified: string;
}

export interface RfxCriterion {
  title: string;
  weight: number;
  description: string;
  type: 'technical' | 'commercial';
}

export interface RfxCommitteeMember {
  id: string;
  displayName: string;
  userId?: string | null;
}

export interface CreateRfxRequest {
  rfxType: string;
  category: string;
  title: string;
  department: string;
  description: string;
  estimatedBudget: number;
  currency: string;
  hideBudget: boolean;
  publicationDate: string;
  closingDate: string;
  submissionDeadline: string;
  priority: string;
  tenderBondRequired: boolean;
  contactPerson: string;
  contactEmail: string;
  contactPhone: string;
  scope: string;
  technicalSpecification: string;
  deliverables: string;
  timeline: string;
  requiredDocuments: string[];
  evaluationCriteria: RfxCriterion[];
  minimumScore: number;
  evaluationNotes?: string | null;
  committeeMemberIds: string[];
  workflowId?: string | null;
  status: 'Draft' | 'Published' | 'Closed';
}
