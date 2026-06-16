import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-error',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="alert alert-error" *ngIf="message">
      {{ message }}
    </div>
  `
})
export class ErrorComponent {
  @Input() message: string | null = null;
}
