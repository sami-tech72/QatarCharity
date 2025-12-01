import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface WorkflowStage {
  name: string;
  owner: string;
  sla: string;
  automation: string;
}

@Component({
  selector: 'app-workflow-configuration-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './workflow-configuration.component.html',
  styleUrl: './workflow-configuration.component.scss',
})
export class WorkflowConfigurationComponent {
  stages: WorkflowStage[] = [
    { name: 'Submission', owner: 'Requester', sla: 'Same day', automation: 'Pre-validation rules' },
    { name: 'Review', owner: 'Functional approver', sla: '48 hours', automation: 'Dynamic routing by category' },
    { name: 'Finance', owner: 'Finance team', sla: '72 hours', automation: 'Budget and threshold checks' },
    { name: 'Executive', owner: 'Leadership', sla: '24 hours', automation: 'Sign-off bundling by project' },
    { name: 'Archive', owner: 'Records', sla: 'Immediate', automation: 'Retention tagging + alerts' },
  ];

  trackByName(_: number, stage: WorkflowStage): string {
    return stage.name;
  }
}
