import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, BehaviorSubject } from 'rxjs';
import { AuthResponse, RegisterRequest, LoginRequest } from '../models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5235/api/auth';
  isAuthenticated = signal(false);
  userEmail = signal<string | null>(null);
  userFullName = signal<string | null>(null);

  private tokenSubject = new BehaviorSubject<string | null>(null);

  constructor(private http: HttpClient) {
    const token = localStorage.getItem('token');
    if (token) {
      this.isAuthenticated.set(true);
      this.userEmail.set(localStorage.getItem('email'));
      this.userFullName.set(localStorage.getItem('fullName'));
      this.tokenSubject.next(token);
    }
  }

  register(req: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, req).pipe(
      tap(res => this.handleAuthResponse(res))
    );
  }

  login(req: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, req).pipe(
      tap(res => this.handleAuthResponse(res))
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('email');
    localStorage.removeItem('fullName');
    this.isAuthenticated.set(false);
    this.userEmail.set(null);
    this.userFullName.set(null);
    this.tokenSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  private handleAuthResponse(res: AuthResponse): void {
    localStorage.setItem('token', res.token);
    localStorage.setItem('email', res.email);
    localStorage.setItem('fullName', res.fullName);
    this.isAuthenticated.set(true);
    this.userEmail.set(res.email);
    this.userFullName.set(res.fullName);
    this.tokenSubject.next(res.token);
  }
}
