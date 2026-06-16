import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, Router } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink],
  template: `
    <header>
      <div class="container">
        <nav>
          <span class="nav-brand" (click)="goHome()">✈ SkyRoute</span>
          <div class="nav-links">
            <a routerLink="/flights">Search Flights</a>
            <ng-container *ngIf="!authService.isAuthenticated()">
              <a routerLink="/register">Register</a>
              <a routerLink="/login">Login</a>
            </ng-container>
            <ng-container *ngIf="authService.isAuthenticated()">
              <span class="nav-user">{{ authService.userFullName() || authService.userEmail() }}</span>
              <button class="nav-logout" (click)="logout()">Logout</button>
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
