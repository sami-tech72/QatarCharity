import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { ApiResponse } from '../../shared/models/api-response.model';
import { PagedResult } from '../../shared/models/pagination.model';
import { CreateRfxRequest, RfxDetail, RfxQueryRequest, RfxSummary } from '../../shared/models/rfx.model';
import { ApiService } from './api.service';

interface RfxSummaryResponse {
  id: string;
  referenceNumber: string;
  title: string;
  category: string;
  status: string;
  committeeStatus: string;
  closingDate: string;
  estimatedBudget: number;
  currency: string;
  workflowName: string | null;
  canApprove: boolean;
}

interface RfxEvaluationCriterionResponse {
  id: string;
  title: string;
  weight: number;
  description: string;
  type: 'technical' | 'commercial';
}

interface RfxCommitteeMemberResponse {
  id: string;
  displayName: string;
  userId: string | null;
}

interface RfxDetailResponse extends Omit<RfxSummaryResponse, 'committeeStatus' | 'workflowName'> {
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
  evaluationNotes: string | null;
  workflowId: string | null;
  evaluationCriteria: RfxEvaluationCriterionResponse[];
  committeeMembers: RfxCommitteeMemberResponse[];
  createdAt: string;
  lastModified: string;
  workflowName: string | null;
}

@Injectable({ providedIn: 'root' })
export class RfxService {
  constructor(private readonly api: ApiService) {}

  loadRfx(query: RfxQueryRequest): Observable<PagedResult<RfxSummary>> {
    return this.api
      .get<PagedResult<RfxSummaryResponse>>('rfx', {
        params: {
          pageNumber: query.pageNumber,
          pageSize: query.pageSize,
          search: query.search ?? '',
          assignedOnly: query.assignedOnly ?? false,
        },
      })
      .pipe(
        map((response) => this.unwrap(response)),
        map((result) => ({
          ...result,
          items: result.items.map((item) => this.mapSummary(item)),
        })),
      );
  }

  createRfx(request: CreateRfxRequest): Observable<RfxDetail> {
    return this.api.post<RfxDetailResponse>('rfx', request).pipe(
      map((response) => this.unwrap(response)),
      map((detail) => this.mapDetail(detail)),
    );
  }

  approveRfx(id: string): Observable<RfxDetail> {
    return this.api.post<RfxDetailResponse>(`rfx/${id}/approve`, {}).pipe(
      map((response) => this.unwrap(response)),
      map((detail) => this.mapDetail(detail)),
    );
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data === undefined || response.data === null) {
      throw new Error(response.message || 'Request failed.');
    }

    return response.data;
  }

  private mapSummary(response: RfxSummaryResponse): RfxSummary {
    return {
      id: response.id,
      referenceNumber: response.referenceNumber,
      title: response.title,
      category: response.category,
      status: response.status,
      committeeStatus: response.committeeStatus,
      canApprove: response.canApprove,
      closingDate: this.toIsoString(response.closingDate),
      estimatedBudget: response.estimatedBudget,
      currency: response.currency,
      workflowName: response.workflowName,
    };
  }

  private mapDetail(response: RfxDetailResponse): RfxDetail {
    const committeeStatus = this.resolveCommitteeStatus(response.status, response.committeeMembers);

    return {
      id: response.id,
      referenceNumber: response.referenceNumber,
      title: response.title,
      category: response.category,
      status: response.status,
      committeeStatus,
      canApprove: response.canApprove,
      closingDate: this.toIsoString(response.closingDate),
      estimatedBudget: response.estimatedBudget,
      currency: response.currency,
      workflowName: response.workflowName,
      rfxType: response.rfxType,
      department: response.department,
      description: response.description,
      hideBudget: response.hideBudget,
      publicationDate: this.toIsoString(response.publicationDate),
      submissionDeadline: this.toIsoString(response.submissionDeadline),
      priority: response.priority,
      tenderBondRequired: response.tenderBondRequired,
      contactPerson: response.contactPerson,
      contactEmail: response.contactEmail,
      contactPhone: response.contactPhone,
      scope: response.scope,
      technicalSpecification: response.technicalSpecification,
      deliverables: response.deliverables,
      timeline: response.timeline,
      requiredDocuments: response.requiredDocuments,
      minimumScore: response.minimumScore,
      evaluationNotes: response.evaluationNotes,
      workflowId: response.workflowId,
      evaluationCriteria: response.evaluationCriteria,
      committeeMembers: response.committeeMembers,
      createdAt: this.toIsoString(response.createdAt),
      lastModified: this.toIsoString(response.lastModified),
    };
  }

  private resolveCommitteeStatus(status: string, committeeMembers: RfxCommitteeMemberResponse[]): string {
    if (status.toLowerCase() === 'published') {
      return 'Approved';
    }

    return committeeMembers.length > 0 ? 'Assigned' : 'Pending';
  }

  private toIsoString(value: string): string {
    try {
      return new Date(value).toISOString();
    } catch {
      return value;
    }
  }
}
