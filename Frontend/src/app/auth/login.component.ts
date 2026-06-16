import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="container">
      <div class="card" style="max-width: 400px; margin: 60px auto;">
        <h2>Login to SkyRoute</h2>
        
        <div class="alert alert-error" *ngIf="error()">
          {{ error() }}
        </div>

        <form [formGroup]="form" (ngSubmit)="login()">
          <div class="form-group">
            <label>Email</label>
            <input type="email" formControlName="email" placeholder="you@example.com">
            <div class="error" *ngIf="form.get('email')?.hasError('required') && form.get('email')?.touched">
              Email is required
            </div>
          </div>

          <div class="form-group">
            <label>Password</label>
            <input type="password" formControlName="password" placeholder="Enter your password">
            <div class="error" *ngIf="form.get('password')?.hasError('required') && form.get('password')?.touched">
              Password is required
            </div>
          </div>

          <button type="submit" [disabled]="!form.valid || loading()" style="width: 100%;">
            {{ loading() ? 'Logging in...' : 'Login' }}
          </button>

          <div style="text-align: center; margin-top: 16px;">
            Don't have an account? <a href="/register" style="color: #1e3a8a; text-decoration: none;">Register</a>
          </div>
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
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Login failed');
      }
    });
  }
}
