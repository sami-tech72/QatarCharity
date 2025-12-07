import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ProcurementPermissionAction } from '../../../shared/models/procurement-roles.model';
import { UserRole } from '../../../shared/models/user.model';

export interface SidebarMenuItem {
  title: string;
  icon: string;
  path: string;
  permission?: {
    name: string;
    action?: ProcurementPermissionAction;
  };
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
})
export class SidebarComponent {
  @Input({ required: true }) currentRole!: UserRole;
  @Input({ required: true }) menuItems: SidebarMenuItem[] = [];
  @Input({ required: true }) roles: UserRole[] = [];

  @Output() roleChange = new EventEmitter<UserRole>();

  onRoleSelect(role: UserRole) {
    this.roleChange.emit(role);
  }
}
