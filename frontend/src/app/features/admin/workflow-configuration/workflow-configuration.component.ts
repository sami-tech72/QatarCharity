import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

type WorkflowStatus = 'Active' | 'Draft' | 'Archived';

type StepType = 'Approval' | 'Task' | 'Notification' | '';

interface Workflow {
  name: string;
  stages: number;
  status: WorkflowStatus;
  lastModified: string;
}

interface WorkflowDraft {
  name: string;
  appliesTo: string;
  description: string;
  steps: StepDraft[];
}

interface StepDraft {
  name: string;
  stepType: StepType;
  assignee: string;
}

@Component({
  selector: 'app-workflow-configuration',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './workflow-configuration.component.html',
  styleUrl: './workflow-configuration.component.scss',
})
export class WorkflowConfigurationComponent {
  readonly workflows: Workflow[] = [
    {
      name: 'Donation Approval',
      stages: 5,
      status: 'Active',
      lastModified: '2024-06-12T00:00:00Z',
    },
    {
      name: 'Volunteer Onboarding',
      stages: 3,
      status: 'Draft',
      lastModified: '2024-05-28T00:00:00Z',
    },
    {
      name: 'Expense Reimbursement',
      stages: 4,
      status: 'Active',
      lastModified: '2024-06-05T00:00:00Z',
    },
    {
      name: 'Grant Application Review',
      stages: 6,
      status: 'Archived',
      lastModified: '2024-04-18T00:00:00Z',
    },
  ];

  readonly statusBadgeClasses: Record<WorkflowStatus, string> = {
    Active: 'badge-light-success',
    Draft: 'badge-light-warning',
    Archived: 'badge-light-secondary',
  };

  readonly appliesToOptions = ['Procurement', 'Finance', 'Programs', 'HR'];
  readonly stepTypes: Exclude<StepType, ''>[] = ['Approval', 'Task', 'Notification'];
  readonly assigneeOptions = ['Procurement Manager', 'Finance Officer', 'Project Lead', 'HR Team'];

  showWorkflowModal = false;
  workflowModalTitle = 'Create New Workflow';

  showStepModal = false;
  editingStepIndex: number | null = null;

  workflowDraft: WorkflowDraft = this.createDefaultWorkflowDraft();
  stepDraft: StepDraft = this.createDefaultStepDraft();

  trackByWorkflowName(_: number, workflow: Workflow): string {
    return workflow.name;
  }

  openCreateWorkflow(): void {
    this.workflowModalTitle = 'Create New Workflow';
    this.workflowDraft = this.createDefaultWorkflowDraft();
    this.showWorkflowModal = true;
  }

  onEdit(workflow: Workflow): void {
    this.workflowModalTitle = `Edit ${workflow.name}`;
    this.workflowDraft = {
      name: workflow.name,
      appliesTo: 'Procurement',
      description: 'Update stages and assignments as needed.',
      steps: [
        {
          name: 'Initial Review',
          stepType: 'Approval',
          assignee: 'Procurement Manager',
        },
        {
          name: 'Budget Check',
          stepType: 'Task',
          assignee: 'Finance Officer',
        },
      ],
    };
    this.showWorkflowModal = true;
  }

  onDelete(workflow: Workflow): void {
    console.log('Delete workflow', workflow.name);
  }

  addStage(): void {
    this.workflowDraft.steps.push(this.createDefaultStepDraft());
    this.openStepModal(this.workflowDraft.steps.length - 1);
  }

  removeStage(index: number): void {
    this.workflowDraft.steps.splice(index, 1);
  }

  openStepModal(index: number): void {
    this.editingStepIndex = index;
    this.stepDraft = { ...this.workflowDraft.steps[index] };
    this.showStepModal = true;
  }

  closeWorkflowModal(): void {
    this.showWorkflowModal = false;
  }

  closeStepModal(): void {
    this.showStepModal = false;
    this.editingStepIndex = null;
  }

  saveStep(): void {
    if (this.editingStepIndex === null) {
      return;
    }

    this.workflowDraft.steps[this.editingStepIndex] = { ...this.stepDraft };
    this.closeStepModal();
  }

  private createDefaultWorkflowDraft(): WorkflowDraft {
    return {
      name: '',
      appliesTo: '',
      description: '',
      steps: [
        {
          name: 'Initial Review',
          stepType: 'Approval',
          assignee: 'Procurement Manager',
        },
        {
          name: 'Assign to Reviewer',
          stepType: 'Task',
          assignee: 'Project Lead',
        },
      ],
    };
  }

  private createDefaultStepDraft(): StepDraft {
    return {
      name: '',
      stepType: '',
      assignee: '',
    };
  }
}
