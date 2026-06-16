import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="empty-state">
      <h3>{{ title }}</h3>
      <p>{{ message }}</p>
    </div>
  `
})
export class EmptyStateComponent {
  @Input() title = 'No data available';
  @Input() message = 'Try searching or adjusting your criteria.';
}
