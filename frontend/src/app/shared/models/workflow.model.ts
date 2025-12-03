import { PageRequest } from './pagination.model';

export type WorkflowStatus = 'Active' | 'Draft' | 'Archived';
export type StepType = 'Approval' | 'Task' | 'Notification';

export interface WorkflowSummary {
  id: string;
  name: string;
  description?: string | null;
  status: WorkflowStatus;
  numberOfStages: number;
  lastModified: string;
}

export interface WorkflowStep {
  id: string;
  name: string;
  stepType: StepType;
  assigneeId: string | null;
  assigneeName: string | null;
  order: number;
}

export interface WorkflowDetail extends WorkflowSummary {
  steps: WorkflowStep[];
}

export interface WorkflowStepRequest {
  name: string;
  stepType: StepType;
  assigneeId: string | null;
  order: number;
}

export interface UpsertWorkflowRequest {
  name: string;
  description?: string | null;
  status: WorkflowStatus;
  steps: WorkflowStepRequest[];
}

export interface WorkflowQueryRequest extends PageRequest {}
