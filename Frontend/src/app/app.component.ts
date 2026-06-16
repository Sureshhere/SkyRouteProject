import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  template: `
    <header>
      <div class="container">
        <nav>
          <h1 style="cursor: pointer;" (click)="goHome()">✈️ SkyRoute</h1>
          <div>
            <a href="/flights">Search</a>
            <ng-container *ngIf="!authService.isAuthenticated()">
              <a href="/register">Register</a>
              <a href="/login">Login</a>
            </ng-container>
            <ng-container *ngIf="authService.isAuthenticated()">
              <span style="color: white; margin-right: 16px;">{{ authService.userEmail() }}</span>
              <button (click)="logout()">Logout</button>
            </ng-container>
          </div>
        </nav>
      </div>
    </header>
    <router-outlet></router-outlet>
  `
})
export class AppComponent {
  constructor(public authService: AuthService, private router: Router) {}

  goHome(): void {
    this.router.navigate(['/flights']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
