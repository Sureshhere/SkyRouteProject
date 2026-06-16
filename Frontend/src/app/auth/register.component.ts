import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

function passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value as string;
  if (!value) return null;
  const errors: ValidationErrors = {};
  if (!/[A-Z]/.test(value)) errors['noUppercase'] = true;
  if (!/[a-z]/.test(value)) errors['noLowercase'] = true;
  if (!/[0-9]/.test(value)) errors['noDigit'] = true;
  return Object.keys(errors).length > 0 ? errors : null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-card">
        <h2 class="auth-title">Create Account</h2>
        <p class="auth-subtitle">Join SkyRoute and start booking flights</p>

        <div class="alert alert-error" *ngIf="error()">
          {{ error() }}
        </div>

        <form [formGroup]="form" (ngSubmit)="register()">
          <div class="form-group">
            <label>Full Name</label>
            <input type="text" formControlName="fullName" placeholder="e.g. John Doe">
            <div class="error" *ngIf="form.get('fullName')?.hasError('required') && form.get('fullName')?.touched">
              Full name is required
            </div>
          </div>

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
            <input type="password" formControlName="password" placeholder="Create a strong password">
            <div class="error" *ngIf="form.get('password')?.hasError('required') && form.get('password')?.touched">
              Password is required
            </div>
            <div class="error" *ngIf="form.get('password')?.hasError('minlength') && form.get('password')?.touched">
              Password must be at least 8 characters
            </div>
            <div class="error" *ngIf="form.get('password')?.hasError('noUppercase') && form.get('password')?.touched">
              Password must contain at least one uppercase letter (A–Z)
            </div>
            <div class="error" *ngIf="form.get('password')?.hasError('noLowercase') && form.get('password')?.touched">
              Password must contain at least one lowercase letter (a–z)
            </div>
            <div class="error" *ngIf="form.get('password')?.hasError('noDigit') && form.get('password')?.touched">
              Password must contain at least one number (0–9)
            </div>
          </div>

          <button type="submit" class="btn-auth" [disabled]="!form.valid || loading()">
            {{ loading() ? 'Creating...' : 'Create Account' }}
          </button>

          <p style="text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px;">
            Already have an account? <a routerLink="/login" class="auth-link">Login</a>
          </p>
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
      password: ['', [Validators.required, Validators.minLength(8), passwordStrengthValidator]]
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
      error: (err: any) => {
        this.loading.set(false);
        if (Array.isArray(err.error?.errors) && err.error.errors.length > 0) {
          this.error.set(err.error.errors.join(' '));
        } else {
          this.error.set(err.error?.error || err.error?.message || 'Registration failed. Please try again.');
        }
      }
    });
  }
}
