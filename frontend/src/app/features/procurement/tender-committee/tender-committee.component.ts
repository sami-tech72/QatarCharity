import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';

interface CommitteeMember {
  id: string;
  name: string;
  role: string;
  department: string;
  email: string;
  phone: string;
  focusArea: string;
  status: MemberStatus;
  connectedUser?: string;
  lastEngagement: string;
}

type MemberStatus = 'Active' | 'Pending' | 'Inactive';

interface UserOption {
  id: string;
  name: string;
  email: string;
  role: string;
  department: string;
}

interface WorkflowStep {
  title: string;
  description: string;
  icon: string;
}

@Component({
  selector: 'app-tender-committee',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './tender-committee.component.html',
  styleUrl: './tender-committee.component.scss',
})
export class TenderCommitteeComponent {
  private readonly fb = inject(FormBuilder);

  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly statusFilter = new FormControl<'all' | MemberStatus>('all', { nonNullable: true });

  readonly availableUsers: UserOption[] = [
    {
      id: 'usr-001',
      name: 'Fatima Al Thani',
      email: 'fatima.althani@qcharity.qa',
      role: 'Procurement Lead',
      department: 'Procurement',
    },
    {
      id: 'usr-002',
      name: 'Omar Hassan',
      email: 'omar.hassan@qcharity.qa',
      role: 'Finance Partner',
      department: 'Finance',
    },
    {
      id: 'usr-003',
      name: 'Layla Mohammed',
      email: 'layla.mohammed@qcharity.qa',
      role: 'Technical Reviewer',
      department: 'Programs Delivery',
    },
    {
      id: 'usr-004',
      name: 'Nasser Ali',
      email: 'nasser.ali@qcharity.qa',
      role: 'Operations',
      department: 'Field Operations',
    },
    {
      id: 'usr-005',
      name: 'Sara Al Kuwari',
      email: 'sara.alkuwari@qcharity.qa',
      role: 'Compliance',
      department: 'Audit & Compliance',
    },
  ];

  members: CommitteeMember[] = [
    {
      id: 'CM-001',
      name: 'Fatima Al Thani',
      role: 'Chairperson',
      department: 'Procurement',
      email: 'fatima.althani@qcharity.qa',
      phone: '+974 5555 1212',
      focusArea: 'Overall governance and approvals',
      status: 'Active',
      connectedUser: 'usr-001',
      lastEngagement: 'Reviewed supplier shortlist today',
    },
    {
      id: 'CM-002',
      name: 'Omar Hassan',
      role: 'Finance Representative',
      department: 'Finance',
      email: 'omar.hassan@qcharity.qa',
      phone: '+974 5555 2323',
      focusArea: 'Budget alignment and value-for-money checks',
      status: 'Active',
      connectedUser: 'usr-002',
      lastEngagement: 'Flagged clarifications on pricing',
    },
    {
      id: 'CM-003',
      name: 'Layla Mohammed',
      role: 'Technical Evaluator',
      department: 'Programs Delivery',
      email: 'layla.mohammed@qcharity.qa',
      phone: '+974 5555 8989',
      focusArea: 'Technical scoring for program deliverables',
      status: 'Pending',
      connectedUser: 'usr-003',
      lastEngagement: 'Awaiting access to evaluation templates',
    },
    {
      id: 'CM-004',
      name: 'Nasser Ali',
      role: 'Field Operations',
      department: 'Field Operations',
      email: 'nasser.ali@qcharity.qa',
      phone: '+974 5555 6767',
      focusArea: 'Operational readiness and logistics',
      status: 'Inactive',
      lastEngagement: 'Rejoin planned after site visit next week',
    },
  ];

  readonly workflowSteps: WorkflowStep[] = [
    {
      title: 'Define committee and roles',
      description: 'Confirm chairs, evaluators, and supporting roles for this tender.',
      icon: 'ki-duotone ki-profile-user',
    },
    {
      title: 'Connect to user accounts',
      description: 'Link members to platform users so they can access RFx artifacts.',
      icon: 'ki-duotone ki-user-square',
    },
    {
      title: 'Share evaluation plan',
      description: 'Send timelines, scoring templates, and responsibilities to the team.',
      icon: 'ki-duotone ki-calendar-8',
    },
    {
      title: 'Track readiness',
      description: 'Monitor who has confirmed availability and is ready to score bids.',
      icon: 'ki-duotone ki-check-square',
    },
  ];

  connectionSelections: Record<string, string> = {};

  readonly memberForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(80)]],
    role: ['', [Validators.required, Validators.maxLength(80)]],
    department: ['', [Validators.required, Validators.maxLength(80)]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.required, Validators.maxLength(30)]],
    focusArea: ['', [Validators.required, Validators.maxLength(120)]],
    status: this.fb.nonNullable.control<MemberStatus>('Pending'),
    connectedUser: this.fb.control<string | null>(null),
  });

  get filteredMembers(): CommitteeMember[] {
    const term = this.searchControl.value.trim().toLowerCase();
    const status = this.statusFilter.value;

    return this.members.filter((member) => {
      const matchesStatus = status === 'all' || member.status === status;
      const matchesTerm =
        !term ||
        [
          member.name,
          member.role,
          member.department,
          member.email,
          member.focusArea,
          member.connectedUser ? this.getUserEmail(member.connectedUser) : '',
        ]
          .filter(Boolean)
          .some((field) => field.toLowerCase().includes(term));

      return matchesStatus && matchesTerm;
    });
  }

  get statusCounts(): Record<MemberStatus, number> {
    return this.members.reduce(
      (counts, member) => ({ ...counts, [member.status]: counts[member.status] + 1 }),
      { Active: 0, Pending: 0, Inactive: 0 }
    );
  }

  get readinessLabel(): string {
    const active = this.statusCounts.Active;
    const pending = this.statusCounts.Pending;

    if (active >= 3 && pending === 0) {
      return 'Committee is ready to begin evaluations';
    }

    if (active >= 2) {
      return 'Core team is set, finalize pending invites';
    }

    return 'Add and connect members to start the tender review';
  }

  getStatusBadgeClass(status: MemberStatus): string {
    switch (status) {
      case 'Active':
        return 'badge-light-success';
      case 'Pending':
        return 'badge-light-warning';
      default:
        return 'badge-light-secondary';
    }
  }

  getUserEmail(userId: string): string {
    return this.availableUsers.find((user) => user.id === userId)?.email ?? '';
  }

  onUserSelection(userId: string): void {
    const selected = this.availableUsers.find((user) => user.id === userId);

    this.memberForm.patchValue({
      connectedUser: userId || null,
      name: selected?.name ?? this.memberForm.value.name ?? '',
      email: selected?.email ?? this.memberForm.value.email ?? '',
      department: selected?.department ?? this.memberForm.value.department ?? '',
      role: selected?.role ?? this.memberForm.value.role ?? '',
    });
  }

  updateMemberStatus(memberId: string, status: MemberStatus): void {
    this.members = this.members.map((member) =>
      member.id === memberId ? { ...member, status, lastEngagement: member.lastEngagement } : member
    );
  }

  onConnectSelectionChange(memberId: string, userId: string): void {
    this.connectionSelections = { ...this.connectionSelections, [memberId]: userId };
  }

  connectUser(memberId: string): void {
    const userId = this.connectionSelections[memberId];
    const user = this.availableUsers.find((option) => option.id === userId);

    if (!user) {
      return;
    }

    this.members = this.members.map((member) =>
      member.id === memberId
        ? {
            ...member,
            connectedUser: user.id,
            name: member.name || user.name,
            email: member.email || user.email,
            department: member.department || user.department,
          }
        : member
    );
  }

  onSubmit(): void {
    if (this.memberForm.invalid) {
      this.memberForm.markAllAsTouched();
      return;
    }

    const raw = this.memberForm.getRawValue();
    const selectedUser = raw.connectedUser
      ? this.availableUsers.find((user) => user.id === raw.connectedUser)
      : null;

    const newMember: CommitteeMember = {
      id: this.generateMemberId(),
      name: raw.name || selectedUser?.name || 'New committee member',
      role: raw.role || selectedUser?.role || 'Evaluator',
      department: raw.department || selectedUser?.department || 'Procurement',
      email: raw.email || selectedUser?.email || '',
      phone: raw.phone ?? '',
      focusArea: raw.focusArea ?? '',
      status: raw.status,
      connectedUser: raw.connectedUser || undefined,
      lastEngagement: 'Added to committee just now',
    };

    this.members = [newMember, ...this.members];
    this.memberForm.reset({
      name: '',
      role: '',
      department: '',
      email: '',
      phone: '',
      focusArea: '',
      status: 'Pending',
      connectedUser: null,
    });
  }

  trackByMemberId(_: number, member: CommitteeMember): string {
    return member.id;
  }

  private generateMemberId(): string {
    const nextNumber = this.members.length + 1;
    return `CM-${nextNumber.toString().padStart(3, '0')}`;
  }
}
