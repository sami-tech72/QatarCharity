import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { UserRole, UserSession } from '../../../shared/models/user.model';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './header.component.html',
})
export class HeaderComponent {
  @Input() title = '';
  @Input() session: UserSession | null = null;
  @Input() roles: UserRole[] = [];
  @Input() currentRole: UserRole | '' = '';

  @Output() roleChange = new EventEmitter<UserRole>();
  @Output() logout = new EventEmitter<void>();

  onRoleChange(role: UserRole) {
    this.roleChange.emit(role);
  }

  onLogout() {
    this.logout.emit();
  }
}
