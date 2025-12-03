import { Injectable } from '@angular/core';
import { ToastrService, IndividualConfig } from 'ngx-toastr';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly toastDelay = 300;
  private readonly config: Partial<IndividualConfig> = {
    closeButton: true,
    timeOut: 4000,
    positionClass: 'toast-top-right',
    progressBar: true,
  };

  constructor(private readonly toastr: ToastrService) {}

  success(message: string, title = 'Success') {
    setTimeout(() => this.toastr.success(message, title, this.config), this.toastDelay);
  }

  error(message: string, title = 'Error') {
    setTimeout(() => this.toastr.error(message, title, this.config), this.toastDelay);
  }

  info(message: string, title = 'Notice') {
    setTimeout(() => this.toastr.info(message, title, this.config), this.toastDelay);
  }
}
