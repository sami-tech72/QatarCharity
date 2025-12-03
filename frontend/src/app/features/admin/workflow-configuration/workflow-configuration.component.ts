import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { WorkflowService } from '../../../core/services/workflow.service';
import { NotificationService } from '../../../core/services/notification.service';
import { UserManagementService } from '../../../core/services/user-management.service';
import { PagedResult } from '../../../shared/models/pagination.model';
import { ManagedUser } from '../../../shared/models/user-management.model';
import {
  StepType,
  UpsertWorkflowRequest,
  WorkflowDetail,
  WorkflowQueryRequest,
  WorkflowStatus,
  WorkflowSummary,
} from '../../../shared/models/workflow.model';

interface WorkflowDraft {
  name: string;
  description: string;
  status: WorkflowStatus;
  steps: StepDraft[];
}

interface StepDraft {
  name: string;
  stepType: StepType | '';
  assigneeId: string | null;
  order: number;
}

@Component({
  selector: 'app-workflow-configuration',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './workflow-configuration.component.html',
  styleUrl: './workflow-configuration.component.scss',
})
export class WorkflowConfigurationComponent implements OnInit {
  private readonly workflowService = inject(WorkflowService);
  private readonly userService = inject(UserManagementService);
  private readonly notifier = inject(NotificationService);

  readonly statusBadgeClasses: Record<WorkflowStatus, string> = {
    Active: 'badge-light-success',
    Draft: 'badge-light-warning',
    Archived: 'badge-light-secondary',
  };

  readonly workflowStatuses: WorkflowStatus[] = ['Active', 'Draft', 'Archived'];
  readonly stepTypes: StepType[] = ['Approval', 'Task', 'Notification'];

  workflows: WorkflowSummary[] = [];
  workflowsPage: PagedResult<WorkflowSummary> | null = null;
  paginationState: WorkflowQueryRequest = {
    pageNumber: 1,
    pageSize: 10,
    search: '',
  };

  assigneeOptions: ManagedUser[] = [];
  private assigneeLookup = new Map<string, string>();

  showWorkflowModal = false;
  workflowModalTitle = 'Create New Workflow';
  showStepModal = false;
  editingStepIndex: number | null = null;
  editingWorkflowId: string | null = null;

  workflowDraft: WorkflowDraft = this.createDefaultWorkflowDraft();
  stepDraft: StepDraft = this.createDefaultStepDraft();

  isLoading = false;
  isSubmitting = false;
  deletingIds = new Set<string>();

  ngOnInit(): void {
    this.loadAssignees();
    this.loadWorkflows();
  }

  trackByWorkflowId(_: number, workflow: WorkflowSummary): string {
    return workflow.id;
  }

  openCreateWorkflow(): void {
    this.editingWorkflowId = null;
    this.workflowModalTitle = 'Create New Workflow';
    this.workflowDraft = this.createDefaultWorkflowDraft();
    this.showWorkflowModal = true;
  }

  onEdit(workflow: WorkflowSummary): void {
    this.workflowModalTitle = `Edit ${workflow.name}`;
    this.editingWorkflowId = workflow.id;
    this.loadWorkflowDetail(workflow.id);
  }

  onDelete(workflow: WorkflowSummary): void {
    if (this.deletingIds.has(workflow.id)) {
      return;
    }

    const confirmed = window.confirm(`Delete workflow "${workflow.name}"?`);

    if (!confirmed) {
      return;
    }

    this.deletingIds.add(workflow.id);

    this.workflowService.deleteWorkflow(workflow.id).subscribe({
      next: () => {
        this.notifier.success('Workflow deleted successfully.');
        this.deletingIds.delete(workflow.id);
        this.loadWorkflows();
      },
      error: (error) => {
        this.deletingIds.delete(workflow.id);
        this.notifier.error(this.getErrorMessage(error, 'Unable to delete workflow.'));
      },
    });
  }

  addStage(): void {
    const newStep: StepDraft = {
      ...this.createDefaultStepDraft(),
      name: `New Step ${this.workflowDraft.steps.length + 1}`,
      order: this.workflowDraft.steps.length + 1,
    };

    this.workflowDraft.steps.push(newStep);
    this.openStepModal(this.workflowDraft.steps.length - 1);
  }

  removeStage(index: number): void {
    this.workflowDraft.steps.splice(index, 1);
    this.reindexSteps();
  }

  openStepModal(index: number): void {
    this.editingStepIndex = index;
    this.stepDraft = { ...this.workflowDraft.steps[index] };
    this.showStepModal = true;
  }

  closeWorkflowModal(): void {
    this.showWorkflowModal = false;
    this.editingWorkflowId = null;
    this.workflowDraft = this.createDefaultWorkflowDraft();
  }

  closeStepModal(): void {
    this.showStepModal = false;
    this.editingStepIndex = null;
    this.stepDraft = this.createDefaultStepDraft();
  }

  saveStep(): void {
    if (this.editingStepIndex === null) {
      return;
    }

    this.workflowDraft.steps[this.editingStepIndex] = {
      ...this.stepDraft,
      stepType: this.stepDraft.stepType || 'Approval',
      name: this.stepDraft.name.trim() || `Step ${this.editingStepIndex + 1}`,
      order: this.editingStepIndex + 1,
    };

    this.showStepModal = false;
    this.editingStepIndex = null;
  }

  saveWorkflow(): void {
    if (this.isSubmitting) {
      return;
    }

    this.isSubmitting = true;

    const payload: UpsertWorkflowRequest = {
      name: this.workflowDraft.name.trim(),
      description: this.workflowDraft.description.trim() || null,
      status: this.workflowDraft.status,
      steps: this.workflowDraft.steps.map((step, index) => ({
        name: step.name.trim() || `Step ${index + 1}`,
        stepType: (step.stepType || 'Approval') as StepType,
        assigneeId: step.assigneeId,
        order: index + 1,
      })),
    };

    const request$ = this.editingWorkflowId
      ? this.workflowService.updateWorkflow(this.editingWorkflowId, payload)
      : this.workflowService.createWorkflow(payload);

    request$.subscribe({
      next: (workflow) => {
        this.notifier.success(`Workflow ${this.editingWorkflowId ? 'updated' : 'created'} successfully.`);
        this.isSubmitting = false;
        this.showWorkflowModal = false;
        this.workflowDraft = this.createDefaultWorkflowDraft();
        this.editingWorkflowId = null;
        this.workflowModalTitle = 'Create New Workflow';
        this.upsertLocalWorkflows(workflow);
        this.loadWorkflows();
      },
      error: (error) => {
        this.isSubmitting = false;
        this.notifier.error(this.getErrorMessage(error, 'Unable to save workflow.'));
      },
    });
  }

  getAssigneeName(assigneeId: string | null): string {
    if (!assigneeId) {
      return 'No assignee';
    }

    return this.assigneeLookup.get(assigneeId) ?? 'Unassigned';
  }

  private loadWorkflows(): void {
    this.isLoading = true;

    this.workflowService.loadWorkflows(this.paginationState).subscribe({
      next: (page) => {
        this.workflows = page.items;
        this.workflowsPage = page;
        this.isLoading = false;
      },
      error: (error) => {
        this.notifier.error(this.getErrorMessage(error, 'Failed to load workflows.'));
        this.isLoading = false;
      },
    });
  }

  private loadWorkflowDetail(id: string): void {
    this.workflowService.getWorkflow(id).subscribe({
      next: (workflow) => {
        this.workflowDraft = this.mapToDraft(workflow);
        this.showWorkflowModal = true;
      },
      error: (error) => {
        this.notifier.error(this.getErrorMessage(error, 'Unable to load workflow.'));
      },
    });
  }

  private loadAssignees(): void {
    this.userService
      .loadUsers({ pageNumber: 1, pageSize: 100, search: '' })
      .subscribe({
        next: (page) => {
          this.assigneeOptions = page.items;
          this.assigneeLookup = new Map(page.items.map((user) => [user.id, user.displayName]));
        },
        error: (error) => {
          this.notifier.error(this.getErrorMessage(error, 'Unable to load users.'));
        },
      });
  }

  private upsertLocalWorkflows(workflow: WorkflowDetail): void {
    this.workflows = [
      {
        id: workflow.id,
        name: workflow.name,
        description: workflow.description,
        status: workflow.status,
        numberOfStages: workflow.numberOfStages,
        lastModified: workflow.lastModified,
      },
      ...this.workflows.filter((existing) => existing.id !== workflow.id),
    ];
  }

  private mapToDraft(workflow: WorkflowDetail): WorkflowDraft {
    return {
      name: workflow.name,
      description: workflow.description ?? '',
      status: workflow.status,
      steps: workflow.steps
        .slice()
        .sort((a, b) => a.order - b.order)
        .map((step) => ({
          name: step.name,
          stepType: step.stepType,
          assigneeId: step.assigneeId,
          order: step.order,
        })),
    };
  }

  private reindexSteps(): void {
    this.workflowDraft.steps = this.workflowDraft.steps.map((step, index) => ({
      ...step,
      order: index + 1,
    }));
  }

  private createDefaultWorkflowDraft(): WorkflowDraft {
    return {
      name: '',
      description: '',
      status: 'Draft',
      steps: [
        {
          name: 'Initial Review',
          stepType: 'Approval',
          assigneeId: null,
          order: 1,
        },
      ],
    };
  }

  private createDefaultStepDraft(): StepDraft {
    return {
      name: '',
      stepType: '',
      assigneeId: null,
      order: 0,
    };
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    const apiError = (error as { error?: { message?: string }; message?: string }) || {};

    return apiError.error?.message || apiError.message || fallback;
  }
}
