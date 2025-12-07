import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LoginRequest } from '../../../shared/models/user.model';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

type LoginForm = FormGroup<{
  email: FormControl<string>;
  password: FormControl<string>;
}>;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  readonly form: LoginForm;

  isSubmitting = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly notifier: NotificationService,
  ) {
    this.form = this.fb.nonNullable.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
    });
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    const payload = this.form.value as LoginRequest;

    this.authService.login(payload).subscribe({
      next: (session) => {
        const path = this.authService.defaultPathForRole(session.role, session);
        this.router.navigateByUrl(path);
      },
      error: (error) => {
        const serverMessage = error?.error?.message ?? error?.message;
        this.notifier.error(serverMessage || 'Invalid email or password.');
        this.isSubmitting = false;
      },
      complete: () => {
        this.isSubmitting = false;
      },
    });
  }
}
