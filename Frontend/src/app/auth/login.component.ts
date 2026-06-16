import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-card">
        <h2 class="auth-title">Welcome back</h2>
        <p class="auth-subtitle">Login to your SkyRoute account</p>

        <div class="alert alert-error" *ngIf="error()">
          {{ error() }}
        </div>

        <form [formGroup]="form" (ngSubmit)="login()">
          <div class="form-group">
            <label>Email Address</label>
            <input type="email" formControlName="email" placeholder="you@example.com">
            <div class="error" *ngIf="form.get('email')?.hasError('required') && form.get('email')?.touched">
              Email address is required
            </div>
            <div class="error" *ngIf="form.get('email')?.hasError('email') && form.get('email')?.touched">
              Please enter a valid email address
            </div>
          </div>

          <div class="form-group">
            <label>Password</label>
            <input type="password" formControlName="password" placeholder="Enter your password">
            <div class="error" *ngIf="form.get('password')?.hasError('required') && form.get('password')?.touched">
              Password is required
            </div>
          </div>

          <button type="submit" class="btn-auth" [disabled]="!form.valid || loading()">
            {{ loading() ? 'Logging in...' : 'Login' }}
          </button>

          <p style="text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px;">
            Don't have an account? <a routerLink="/register" class="auth-link">Register</a>
          </p>
        </form>
      </div>
    </div>
  `
})
export class LoginComponent {
  form: FormGroup;
  loading = signal(false);
  error = signal<string | null>(null);

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  login(): void {
    if (!this.form.valid) return;

    this.loading.set(true);
    this.error.set(null);

    this.authService.login(this.form.value).subscribe({
      next: () => {
        this.router.navigate(['/flights']);
      },
      error: (err: any) => {
        this.loading.set(false);
        if (Array.isArray(err.error?.errors) && err.error.errors.length > 0) {
          this.error.set(err.error.errors.join(' '));
        } else {
          this.error.set(err.error?.error || err.error?.message || 'Incorrect email or password. Please try again.');
        }
      }
    });
  }
}
