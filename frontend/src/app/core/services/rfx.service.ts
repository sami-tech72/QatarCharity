import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, defer, map, of } from 'rxjs';
import { delay } from 'rxjs/operators';

import { PagedResult } from '../../shared/models/pagination.model';
import { CreateRfxRequest, RfxDetail, RfxQueryRequest, RfxSummary } from '../../shared/models/rfx.model';

@Injectable({ providedIn: 'root' })
export class RfxService {
  private readonly records$ = new BehaviorSubject<RfxDetail[]>([
    {
      id: 'rfx-001',
      referenceNumber: 'RFx-2024-001',
      title: 'IT Infrastructure Upgrade',
      category: 'Information Technology',
      status: 'Draft',
      committeeStatus: 'Assigned',
      canApprove: true,
      closingDate: '2024-08-15T00:00:00Z',
      estimatedBudget: 2500000,
      currency: 'QAR',
      workflowName: 'Standard Procurement',
      rfxType: 'RFP',
      department: 'IT',
      description: 'Upgrade data center and network backbone',
      hideBudget: false,
      publicationDate: '2024-07-10T00:00:00Z',
      submissionDeadline: '2024-08-15T00:00:00Z',
      priority: 'High',
      tenderBondRequired: true,
      contactPerson: 'Fatima Al-Naimi',
      contactEmail: 'fatima.alnaimi@company.qa',
      contactPhone: '+974 3333 1111',
      scope: 'Data center refresh, network upgrades, and security hardening.',
      technicalSpecification: 'Detailed specs for servers, storage, firewalls, and switches.',
      deliverables: 'Hardware, implementation services, documentation, and knowledge transfer.',
      timeline: 'Project kickoff in September with phased delivery over 6 months.',
      requiredDocuments: ['Company Profile', 'Trade License', 'Methodology'],
      minimumScore: 75,
      evaluationNotes: null,
      evaluationCriteria: [
        { title: 'Technical Compliance', weight: 30, description: 'Alignment with specs', type: 'technical' },
        { title: 'Delivery Approach', weight: 20, description: 'Implementation plan', type: 'technical' },
        { title: 'Commercial Offer', weight: 30, description: 'Cost competitiveness', type: 'commercial' },
        { title: 'Experience', weight: 20, description: 'Relevant references', type: 'technical' },
      ],
      committeeMembers: [
        { id: 'user-1', displayName: 'Aisha Mohammed' },
        { id: 'user-2', displayName: 'Hassan Ali' },
      ],
      createdAt: '2024-07-01T00:00:00Z',
      lastModified: '2024-07-05T00:00:00Z',
    },
    {
      id: 'rfx-002',
      referenceNumber: 'RFx-2024-002',
      title: 'Facilities Management Services',
      category: 'Operations',
      status: 'Published',
      committeeStatus: 'In Review',
      canApprove: false,
      closingDate: '2024-09-01T00:00:00Z',
      estimatedBudget: 1200000,
      currency: 'QAR',
      workflowName: 'Outsourcing Workflow',
      rfxType: 'RFP',
      department: 'Facilities',
      description: 'Comprehensive facilities management for Doha offices.',
      hideBudget: false,
      publicationDate: '2024-07-01T00:00:00Z',
      submissionDeadline: '2024-09-01T00:00:00Z',
      priority: 'Medium',
      tenderBondRequired: true,
      contactPerson: 'Noura Al-Mansouri',
      contactEmail: 'noura.mansouri@company.qa',
      contactPhone: '+974 4444 2222',
      scope: 'Cleaning, maintenance, security, and landscaping services.',
      technicalSpecification: 'Detailed SLA expectations and staffing requirements.',
      deliverables: 'Monthly service reports, incident handling, and preventive maintenance.',
      timeline: 'Service start October for 12 months.',
      requiredDocuments: ['Company Profile', 'Trade License', 'References/Past Projects'],
      minimumScore: 70,
      evaluationNotes: 'Prioritize bidders with regional FM experience.',
      evaluationCriteria: [
        { title: 'Service Model', weight: 25, description: 'Approach and staffing', type: 'technical' },
        { title: 'SLA & Reporting', weight: 20, description: 'KPIs and reporting cadence', type: 'technical' },
        { title: 'Price', weight: 30, description: 'Commercial proposal', type: 'commercial' },
        { title: 'References', weight: 25, description: 'Relevant past performance', type: 'technical' },
      ],
      committeeMembers: [
        { id: 'user-3', displayName: 'Salem Al-Kuwari' },
        { id: 'user-4', displayName: 'Maryam Al-Dosari' },
      ],
      createdAt: '2024-06-15T00:00:00Z',
      lastModified: '2024-07-02T00:00:00Z',
    },
    {
      id: 'rfx-003',
      referenceNumber: 'RFx-2024-003',
      title: 'Logistics Partner Onboarding',
      category: 'Supply Chain',
      status: 'Closed',
      committeeStatus: 'Approved',
      canApprove: false,
      closingDate: '2024-06-30T00:00:00Z',
      estimatedBudget: 800000,
      currency: 'QAR',
      workflowName: 'Strategic Sourcing',
      rfxType: 'RFI',
      department: 'Logistics',
      description: 'Identify logistics partners for GCC distribution.',
      hideBudget: true,
      publicationDate: '2024-05-15T00:00:00Z',
      submissionDeadline: '2024-06-30T00:00:00Z',
      priority: 'High',
      tenderBondRequired: false,
      contactPerson: 'Khalid Al-Kaabi',
      contactEmail: 'khalid.kaabi@company.qa',
      contactPhone: '+974 5555 3333',
      scope: 'Inbound freight, warehousing, and last-mile delivery.',
      technicalSpecification: 'Cold-chain capability, fleet size, and tracking.',
      deliverables: 'Proposed routes, SLAs, and transition plan.',
      timeline: 'Partner selection by August, go-live in Q4.',
      requiredDocuments: ['Company Profile', 'Certifications'],
      minimumScore: 68,
      evaluationNotes: 'Include sustainability track record.',
      evaluationCriteria: [
        { title: 'Network Coverage', weight: 35, description: 'Coverage and capacity', type: 'technical' },
        { title: 'Technology Platform', weight: 20, description: 'Tracking and integrations', type: 'technical' },
        { title: 'Cost Model', weight: 25, description: 'Pricing and surcharges', type: 'commercial' },
        { title: 'Sustainability', weight: 20, description: 'Green initiatives', type: 'technical' },
      ],
      committeeMembers: [
        { id: 'user-5', displayName: 'Lulwa Al-Mannai' },
        { id: 'user-6', displayName: 'Omar Al-Suwaidi' },
      ],
      createdAt: '2024-05-01T00:00:00Z',
      lastModified: '2024-06-30T00:00:00Z',
    },
  ]);

  loadRfx(query: RfxQueryRequest): Observable<PagedResult<RfxSummary>> {
    return defer(() => of(this.records$.value)).pipe(
      map((records) => this.applyQuery(records, query)),
      delay(150),
    );
  }

  createRfx(request: CreateRfxRequest): Observable<RfxDetail> {
    return defer(() => {
      const record = this.mapCreateRequest(request);
      const updated = [record, ...this.records$.value];

      this.records$.next(updated);

      return of(record).pipe(delay(250));
    });
  }

  approveRfx(id: string): Observable<RfxDetail> {
    return defer(() => {
      const current = [...this.records$.value];
      const match = current.find((item) => item.id === id);

      if (!match) {
        throw new Error('RFx record not found.');
      }

      if (!match.canApprove || match.status !== 'Draft') {
        throw new Error('RFx cannot be approved in its current state.');
      }

      match.status = 'Published';
      match.committeeStatus = 'Approved';
      match.canApprove = false;
      match.lastModified = new Date().toISOString();

      this.records$.next(current);

      return of({ ...match }).pipe(delay(200));
    });
  }

  private applyQuery(records: RfxDetail[], query: RfxQueryRequest): PagedResult<RfxSummary> {
    const term = (query.search ?? '').toLowerCase();
    const filtered = records.filter((record) => {
      const matchesSearch =
        !term ||
        record.title.toLowerCase().includes(term) ||
        record.referenceNumber.toLowerCase().includes(term) ||
        record.category.toLowerCase().includes(term);

      const matchesAssignment = !query.assignedOnly || record.committeeStatus !== 'Unassigned';

      return matchesSearch && matchesAssignment;
    });

    const start = (query.pageNumber - 1) * query.pageSize;
    const pagedItems = filtered.slice(start, start + query.pageSize);

    const summaries: RfxSummary[] = pagedItems.map((record) => this.toSummary(record));

    return { items: summaries, totalCount: filtered.length };
  }

  private mapCreateRequest(request: CreateRfxRequest): RfxDetail {
    const nextIndex = this.records$.value.length + 1;
    const referenceNumber = `RFx-2024-${nextIndex.toString().padStart(3, '0')}`;
    const now = new Date().toISOString();

    return {
      id: `rfx-${nextIndex.toString().padStart(3, '0')}`,
      referenceNumber,
      title: request.title,
      category: request.category,
      status: request.status,
      committeeStatus: 'Assigned',
      canApprove: request.status === 'Draft' || request.status === 'Published',
      closingDate: request.closingDate,
      estimatedBudget: request.estimatedBudget,
      currency: request.currency,
      workflowName: request.workflowId ? this.lookupWorkflowName(request.workflowId) : 'Standard Procurement',
      rfxType: request.rfxType,
      department: request.department,
      description: request.description,
      hideBudget: request.hideBudget,
      publicationDate: request.publicationDate,
      submissionDeadline: request.submissionDeadline,
      priority: request.priority,
      tenderBondRequired: request.tenderBondRequired,
      contactPerson: request.contactPerson,
      contactEmail: request.contactEmail,
      contactPhone: request.contactPhone,
      scope: request.scope,
      technicalSpecification: request.technicalSpecification,
      deliverables: request.deliverables,
      timeline: request.timeline,
      requiredDocuments: request.requiredDocuments,
      minimumScore: request.minimumScore,
      evaluationNotes: request.evaluationNotes ?? null,
      evaluationCriteria: request.evaluationCriteria,
      committeeMembers: request.committeeMemberIds.map((id) => ({ id, displayName: id })),
      createdAt: now,
      lastModified: now,
    };
  }

  private lookupWorkflowName(workflowId: string | null | undefined): string | null {
    if (!workflowId) {
      return null;
    }

    const knownWorkflows: Record<string, string> = {
      'workflow-1': 'Standard Procurement',
      'workflow-2': 'Fast-Track Approval',
      'workflow-3': 'Strategic Sourcing',
    };

    return knownWorkflows[workflowId] ?? 'Custom Workflow';
  }

  private toSummary(record: RfxDetail): RfxSummary {
    const { evaluationCriteria: _criteria, committeeMembers: _members, ...summaryFields } = record;
    return summaryFields;
  }
}
