import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-header',
  standalone: true,
  templateUrl: './header.component.html',
})
export class HeaderComponent {
  @Input() title = '';
}
