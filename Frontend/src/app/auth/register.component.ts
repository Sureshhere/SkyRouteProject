import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="container">
      <div class="card" style="max-width: 400px; margin: 60px auto;">
        <h2>Create Account</h2>
        
        <div class="alert alert-error" *ngIf="error()">
          {{ error() }}
        </div>

        <form [formGroup]="form" (ngSubmit)="register()">
          <div class="form-group">
            <label>Full Name</label>
            <input type="text" formControlName="fullName" placeholder="John Doe">
            <div class="error" *ngIf="form.get('fullName')?.hasError('required') && form.get('fullName')?.touched">
              Full name is required
            </div>
          </div>

          <div class="form-group">
            <label>Email</label>
            <input type="email" formControlName="email" placeholder="you@example.com">
            <div class="error" *ngIf="form.get('email')?.hasError('required') && form.get('email')?.touched">
              Email is required
            </div>
            <div class="error" *ngIf="form.get('email')?.hasError('email') && form.get('email')?.touched">
              Invalid email
            </div>
          </div>

          <div class="form-group">
            <label>Password</label>
            <input type="password" formControlName="password" placeholder="Min 6 characters">
            <div class="error" *ngIf="form.get('password')?.hasError('required') && form.get('password')?.touched">
              Password is required
            </div>
            <div class="error" *ngIf="form.get('password')?.hasError('minlength') && form.get('password')?.touched">
              Password must be at least 6 characters
            </div>
          </div>

          <button type="submit" [disabled]="!form.valid || loading()" style="width: 100%;">
            {{ loading() ? 'Creating...' : 'Create Account' }}
          </button>

          <div style="text-align: center; margin-top: 16px;">
            Already have an account? <a href="/login" style="color: #1e3a8a; text-decoration: none;">Login</a>
          </div>
        </form>
      </div>
    </div>
  `
})
export class RegisterComponent {
  form: FormGroup;
  loading = signal(false);
  error = signal<string | null>(null);

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
    this.form = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  register(): void {
    if (!this.form.valid) return;

    this.loading.set(true);
    this.error.set(null);

    this.authService.register(this.form.value).subscribe({
      next: () => {
        this.router.navigate(['/flights']);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Registration failed');
      }
    });
  }
}
