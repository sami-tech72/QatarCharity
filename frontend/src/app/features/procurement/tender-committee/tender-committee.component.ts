import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-tender-committee',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tender-committee.component.html',
  styleUrls: ['./tender-committee.component.scss'],
})
export class TenderCommitteeComponent {
  approvalSteps = [
    {
      icon: 'ki-duotone ki-verify',
      title: 'Committee assignment',
      description:
        'Add the relevant users to the committee from RFx Management > Workflow & Committee so backend workflow rules know who must approve.',
    },
    {
      icon: 'ki-duotone ki-check-circle',
      title: 'Approval required',
      description:
        'Each committee member reviews the RFx and records their decision. The publication stays on hold until the workflow captures every approval.',
    },
    {
      icon: 'ki-duotone ki-information-2',
      title: 'Publication',
      description:
        'Once all approvals are completed, the system publishes the RFx automatically and reflects the status in both frontend and backend records.',
    },
  ];
}
