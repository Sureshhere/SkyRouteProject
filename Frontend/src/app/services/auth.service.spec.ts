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
    it('should send login request with correct endpoint', (done) => {
      const loginReq: LoginRequest = { email: 'user@example.com', password: 'password123' };
      service.login(loginReq).subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
      expect(req.request.method).toBe('POST');
      req.flush({ token: 'test-token', email: 'user@example.com', fullName: 'John Doe' });
    });

    it('should handle login errors with 401 status', (done) => {
      const loginReq: LoginRequest = { email: 'user@example.com', password: 'wrongpassword' };
      service.login(loginReq).subscribe({
        error: (error) => {
          expect(error.status).toBe(401);
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
      req.flush({ error: 'Invalid credentials' }, { status: 401, statusText: 'Unauthorized' });
    });
  });

  describe('Register', () => {
    it('should send register request with correct endpoint', (done) => {
      const registerReq: RegisterRequest = { email: 'newuser@example.com', fullName: 'Jane Smith', password: 'securepassword123' };
      service.register(registerReq).subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/register'));
      expect(req.request.method).toBe('POST');
      req.flush({ token: 'test-token', email: 'newuser@example.com', fullName: 'Jane Smith' });
    });

    it('should handle register errors with 409 status (duplicate email)', (done) => {
      const registerReq: RegisterRequest = { email: 'existing@example.com', fullName: 'John Doe', password: 'password123' };
      service.register(registerReq).subscribe({
        error: (error) => {
          expect(error.status).toBe(409);
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/register'));
      req.flush({ error: 'Email already exists' }, { status: 409, statusText: 'Conflict' });
    });

    it('should handle register errors with 400 status (validation failure)', (done) => {
      const registerReq: RegisterRequest = { email: 'user@example.com', fullName: 'John Doe', password: '123' };
      service.register(registerReq).subscribe({
        error: (error) => {
          expect(error.status).toBe(400);
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/register'));
      req.flush({ error: 'Password too weak' }, { status: 400, statusText: 'Bad Request' });
    });
  });

  describe('Logout', () => {
    it('should be a void method that terminates session', () => {
      const result = service.logout();
      expect(result).toBeUndefined();
    });
  });

  describe('Signal Initialization', () => {
    it('should initialize isAuthenticated signal as false', () => {
      expect(service.isAuthenticated()).toBe(false);
    });

    it('should initialize userEmail signal as null', () => {
      expect(service.userEmail()).toBeNull();
    });

    it('should initialize userFullName signal as null', () => {
      expect(service.userFullName()).toBeNull();
    });
  });

  describe('Token Management', () => {
    it('should return null when token is not available', () => {
      const token = service.getToken();
      expect(token).toBeNull();
    });
  });

  describe('HttpOnly Cookie Security', () => {
    it('should store authentication token in localStorage after successful login', (done) => {
      const loginReq: LoginRequest = { email: 'user@example.com', password: 'password123' };
      service.login(loginReq).subscribe({
        next: () => {
          expect(localStorage.getItem('token')).toBe('test-token');
          expect(localStorage.getItem('email')).toBe('user@example.com');
          expect(localStorage.getItem('fullName')).toBe('John Doe');
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
      req.flush({ token: 'test-token', email: 'user@example.com', fullName: 'John Doe' });
    });

    it('should not have Authorization header in requests', (done) => {
      const loginReq: LoginRequest = { email: 'user@example.com', password: 'password123' };
      service.login(loginReq).subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
      expect(req.request.headers.has('Authorization')).toBe(false);
      req.flush({ token: 'test-token', email: 'user@example.com', fullName: 'John Doe' });
    });
  });
});
