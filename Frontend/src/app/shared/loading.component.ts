import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loading',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="loading-spinner">
      <div class="spinner"></div>
    </div>
  `
})
export class LoadingComponent {}
