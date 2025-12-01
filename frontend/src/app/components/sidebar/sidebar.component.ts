import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

export type SidebarRole = 'Admin' | 'Procurement' | 'Supplier';

export interface SidebarMenuItem {
  title: string;
  icon: string;
  path: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
})
export class SidebarComponent {
  @Input({ required: true }) currentRole!: SidebarRole;
  @Input({ required: true }) menuItems: SidebarMenuItem[] = [];
  @Input({ required: true }) roles: SidebarRole[] = [];

  @Output() roleChange = new EventEmitter<SidebarRole>();

  onRoleSelect(role: SidebarRole) {
    this.roleChange.emit(role);
  }
}
