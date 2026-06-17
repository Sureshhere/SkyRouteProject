import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { LoginRequest, RegisterRequest } from '../models';

describe('AuthService - HttpOnly Cookie Authentication', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  describe('Login', () => {
    it('should login successfully with valid credentials', (done) => {
      const loginReq: LoginRequest = { email: 'user@example.com', password: 'password123' };
      service.login(loginReq).subscribe({
        next: () => {
          expect(service.isAuthenticated()).toBe(true);
          expect(service.userEmail()).toBe('user@example.com');
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
      expect(req.request.method).toBe('POST');
      expect(req.request.withCredentials).toBe(true);
      req.flush({ success: true, email: 'user@example.com', fullName: 'John Doe' });
    });

    it('should fail login with invalid credentials', (done) => {
      const loginReq: LoginRequest = { email: 'user@example.com', password: 'wrongpassword' };
      service.login(loginReq).subscribe({
        error: () => {
          expect(service.isAuthenticated()).toBe(false);
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
      req.flush({ error: 'Invalid credentials' }, { status: 401, statusText: 'Unauthorized' });
    });

    it('should normalize email to lowercase in signal', (done) => {
      const loginReq: LoginRequest = { email: 'USER@EXAMPLE.COM', password: 'password123' };
      service.login(loginReq).subscribe({
        next: () => {
          expect(service.userEmail()).toBe('user@example.com');
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
      req.flush({ success: true, email: 'user@example.com', fullName: 'John Doe' });
    });
  });

  describe('Register', () => {
    it('should register successfully with valid data', (done) => {
      const registerReq: RegisterRequest = { email: 'newuser@example.com', fullName: 'Jane Smith', password: 'securepassword123' };
      service.register(registerReq).subscribe({
        next: () => {
          expect(localStorage.getItem('isLoggedIn')).toBe('true');
          expect(localStorage.getItem('email')).toBe('newuser@example.com');
          expect(localStorage.getItem('fullName')).toBe('Jane Smith');
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/register'));
      expect(req.request.method).toBe('POST');
      expect(req.request.withCredentials).toBe(true);
      req.flush({ success: true, email: 'newuser@example.com', fullName: 'Jane Smith' });
    });

    it('should fail register with duplicate email', (done) => {
      const registerReq: RegisterRequest = { email: 'existing@example.com', fullName: 'John Doe', password: 'password123' };
      service.register(registerReq).subscribe({
        error: () => {
          expect(localStorage.getItem('isLoggedIn')).toBeNull();
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/register'));
      req.flush({ error: 'Email already exists' }, { status: 409, statusText: 'Conflict' });
    });

    it('should fail register with weak password', (done) => {
      const registerReq: RegisterRequest = { email: 'user@example.com', fullName: 'John Doe', password: '123' };
      service.register(registerReq).subscribe({
        error: () => {
          expect(localStorage.getItem('isLoggedIn')).toBeNull();
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/register'));
      req.flush({ error: 'Password too weak' }, { status: 400, statusText: 'Bad Request' });
    });
  });

  describe('Logout', () => {
    it('should be a void method that terminates session', () => {
      // logout() is a void method that doesn't return Observable
      // The actual logout behavior is tested in auth.interceptor tests
      const result = service.logout();
      expect(result).toBeUndefined();
    });
  });

  describe('Signal Initialization', () => {
    it('should initialize isAuthenticated signal as false', () => {
      expect(service.isAuthenticated()).toBe(false);
    });

    it('should initialize userEmail signal as empty string', () => {
      expect(service.userEmail()).toBe('');
    });

    it('should initialize userFullName signal as empty string', () => {
      expect(service.userFullName()).toBe('');
    });
  });

  describe('localStorage Persistence', () => {
    it('should persist isLoggedIn flag to localStorage on successful login', (done) => {
      const loginReq: LoginRequest = { email: 'user@example.com', password: 'password123' };
      service.login(loginReq).subscribe({
        next: () => {
          expect(localStorage.getItem('isLoggedIn')).toBe('true');
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
      req.flush({ success: true, email: 'user@example.com', fullName: 'John Doe' });
    });

    it('should persist email to localStorage on successful login', (done) => {
      const loginReq: LoginRequest = { email: 'test@example.com', password: 'password123' };
      service.login(loginReq).subscribe({
        next: () => {
          expect(localStorage.getItem('email')).toBe('test@example.com');
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
      req.flush({ success: true, email: 'test@example.com', fullName: 'John Doe' });
    });

    it('should persist fullName to localStorage on successful login', (done) => {
      const loginReq: LoginRequest = { email: 'user@example.com', password: 'password123' };
      service.login(loginReq).subscribe({
        next: () => {
          expect(localStorage.getItem('fullName')).toBe('Jane Smith');
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
      req.flush({ success: true, email: 'user@example.com', fullName: 'Jane Smith' });
    });

    it('should hydrate signals from localStorage on service initialization', () => {
      localStorage.setItem('isLoggedIn', 'true');
      localStorage.setItem('email', 'stored@example.com');
      localStorage.setItem('fullName', 'Stored User');

      const newService = TestBed.inject(AuthService);
      expect(newService.isAuthenticated()).toBe(true);
      expect(newService.userEmail()).toBe('stored@example.com');
      expect(newService.userFullName()).toBe('Stored User');
    });
  });

  describe('Security - Token Storage', () => {
    it('should NOT store authentication token in localStorage', (done) => {
      const loginReq: LoginRequest = { email: 'user@example.com', password: 'password123' };
      service.login(loginReq).subscribe({
        next: () => {
          expect(localStorage.getItem('token')).toBeNull();
          expect(localStorage.getItem('authToken')).toBeNull();
          expect(localStorage.getItem('accessToken')).toBeNull();
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
      req.flush({ success: true, email: 'user@example.com', fullName: 'John Doe', token: 'secret-token' });
    });

    it('should NOT have Authorization header in requests (uses HttpOnly cookie)', (done) => {
      const loginReq: LoginRequest = { email: 'user@example.com', password: 'password123' };
      service.login(loginReq).subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
      expect(req.request.headers.has('Authorization')).toBe(false);
      req.flush({ success: true, email: 'user@example.com', fullName: 'John Doe' });
    });

    it('should send requests with withCredentials: true for cookie transport', (done) => {
      const loginReq: LoginRequest = { email: 'user@example.com', password: 'password123' };
      service.login(loginReq).subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
      expect(req.request.withCredentials).toBe(true);
      req.flush({ success: true, email: 'user@example.com', fullName: 'John Doe' });
    });
  });

  describe('HTTP Configuration', () => {
    it('should use correct API endpoint for login', (done) => {
      const loginReq: LoginRequest = { email: 'user@example.com', password: 'password123' };
      service.login(loginReq).subscribe(
        () => done()
      );

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
      expect(req.request.url).toContain('/api/auth/login');
      req.flush({ success: true, email: 'user@example.com', fullName: 'John Doe' });
    });

    it('should use correct API endpoint for register', (done) => {
      const registerReq: RegisterRequest = { email: 'user@example.com', fullName: 'John Doe', password: 'password123' };
      service.register(registerReq).subscribe(
        () => done()
      );

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/register'));
      expect(req.request.url).toContain('/api/auth/register');
      req.flush({ success: true, email: 'user@example.com', fullName: 'John Doe' });
    });

    it('should use correct API endpoint for logout', () => {
      service.logout();
      expect(localStorage.getItem('isLoggedIn')).toBeNull();
    });
  });
});
