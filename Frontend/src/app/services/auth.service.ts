import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AuthResponse, RegisterRequest, LoginRequest } from '../models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5235/api/auth';
  isAuthenticated = signal(false);
  userEmail = signal<string | null>(null);
  userFullName = signal<string | null>(null);

  constructor(private http: HttpClient) {
    if (localStorage.getItem('isLoggedIn') === 'true') {
      this.isAuthenticated.set(true);
      this.userEmail.set(localStorage.getItem('email'));
      this.userFullName.set(localStorage.getItem('fullName'));
    }
  }

  register(req: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, req, { withCredentials: true }).pipe(
      tap(res => this.handleAuthResponse(res))
    );
  }

  login(req: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, req, { withCredentials: true }).pipe(
      tap(res => this.handleAuthResponse(res))
    );
  }

  logout(): void {
    this.http.post(`${this.apiUrl}/logout`, {}, { withCredentials: true }).subscribe({
      complete: () => this.clearSession(),
      error: () => this.clearSession()
    });
  }

  private clearSession(): void {
    localStorage.removeItem('isLoggedIn');
    localStorage.removeItem('email');
    localStorage.removeItem('fullName');
    this.isAuthenticated.set(false);
    this.userEmail.set(null);
    this.userFullName.set(null);
  }

  private handleAuthResponse(res: AuthResponse): void {
    localStorage.setItem('isLoggedIn', 'true');
    localStorage.setItem('email', res.email);
    localStorage.setItem('fullName', res.fullName);
    this.isAuthenticated.set(true);
    this.userEmail.set(res.email);
    this.userFullName.set(res.fullName);
  }
}
