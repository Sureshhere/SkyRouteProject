import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';

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
      service.login('user@example.com', 'password123').subscribe({
        next: () => {
          expect(service.isAuthenticated()).toBe(true);
          expect(service.userEmail()).toBe('user@example.com');
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/login');
      expect(req.request.method).toBe('POST');
      expect(req.request.withCredentials).toBe(true);
      req.flush({ success: true, email: 'user@example.com', fullName: 'John Doe' });
    });

    it('should fail login with invalid credentials', (done) => {
      service.login('user@example.com', 'wrongpassword').subscribe({
        error: () => {
          expect(service.isAuthenticated()).toBe(false);
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/login');
      req.flush({ error: 'Invalid credentials' }, { status: 401, statusText: 'Unauthorized' });
    });

    it('should normalize email to lowercase in signal', (done) => {
      service.login('USER@EXAMPLE.COM', 'password123').subscribe({
        next: () => {
          expect(service.userEmail()).toBe('user@example.com');
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/login');
      req.flush({ success: true, email: 'user@example.com', fullName: 'John Doe' });
    });
  });

  describe('Register', () => {
    it('should register successfully with valid data', (done) => {
      service.register('newuser@example.com', 'Jane Smith', 'securepassword123').subscribe({
        next: () => {
          expect(localStorage.getItem('isLoggedIn')).toBe('true');
          expect(localStorage.getItem('email')).toBe('newuser@example.com');
          expect(localStorage.getItem('fullName')).toBe('Jane Smith');
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/register');
      expect(req.request.method).toBe('POST');
      expect(req.request.withCredentials).toBe(true);
      req.flush({ success: true, email: 'newuser@example.com', fullName: 'Jane Smith' });
    });

    it('should fail register with duplicate email', (done) => {
      service.register('existing@example.com', 'John Doe', 'password123').subscribe({
        error: () => {
          expect(localStorage.getItem('isLoggedIn')).toBeNull();
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/register');
      req.flush({ error: 'Email already exists' }, { status: 409, statusText: 'Conflict' });
    });

    it('should fail register with weak password', (done) => {
      service.register('user@example.com', 'John Doe', '123').subscribe({
        error: () => {
          expect(localStorage.getItem('isLoggedIn')).toBeNull();
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/register');
      req.flush({ error: 'Password too weak' }, { status: 400, statusText: 'Bad Request' });
    });
  });

  describe('Logout', () => {
    it('should logout and clear localStorage', (done) => {
      localStorage.setItem('isLoggedIn', 'true');
      localStorage.setItem('email', 'user@example.com');
      localStorage.setItem('fullName', 'John Doe');

      service.logout().subscribe({
        next: () => {
          expect(localStorage.getItem('isLoggedIn')).toBeNull();
          expect(localStorage.getItem('email')).toBeNull();
          expect(localStorage.getItem('fullName')).toBeNull();
          expect(service.isAuthenticated()).toBe(false);
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/logout');
      expect(req.request.method).toBe('POST');
      expect(req.request.withCredentials).toBe(true);
      req.flush({ success: true });
    });

    it('should call logout endpoint even if error occurs', (done) => {
      localStorage.setItem('isLoggedIn', 'true');

      service.logout().subscribe({
        error: () => {
          expect(localStorage.getItem('isLoggedIn')).toBeNull();
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/logout');
      req.flush({ error: 'Logout failed' }, { status: 500, statusText: 'Internal Server Error' });
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
      service.login('user@example.com', 'password123').subscribe({
        next: () => {
          expect(localStorage.getItem('isLoggedIn')).toBe('true');
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/login');
      req.flush({ success: true, email: 'user@example.com', fullName: 'John Doe' });
    });

    it('should persist email to localStorage on successful login', (done) => {
      service.login('test@example.com', 'password123').subscribe({
        next: () => {
          expect(localStorage.getItem('email')).toBe('test@example.com');
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/login');
      req.flush({ success: true, email: 'test@example.com', fullName: 'John Doe' });
    });

    it('should persist fullName to localStorage on successful login', (done) => {
      service.login('user@example.com', 'password123').subscribe({
        next: () => {
          expect(localStorage.getItem('fullName')).toBe('Jane Smith');
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/login');
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
      service.login('user@example.com', 'password123').subscribe({
        next: () => {
          expect(localStorage.getItem('token')).toBeNull();
          expect(localStorage.getItem('authToken')).toBeNull();
          expect(localStorage.getItem('accessToken')).toBeNull();
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/login');
      req.flush({ success: true, email: 'user@example.com', fullName: 'John Doe', token: 'secret-token' });
    });

    it('should NOT have Authorization header in requests (uses HttpOnly cookie)', (done) => {
      service.login('user@example.com', 'password123').subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/login');
      expect(req.request.headers.has('Authorization')).toBe(false);
      req.flush({ success: true, email: 'user@example.com', fullName: 'John Doe' });
    });

    it('should send requests with withCredentials: true for cookie transport', (done) => {
      service.login('user@example.com', 'password123').subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/login');
      expect(req.request.withCredentials).toBe(true);
      req.flush({ success: true, email: 'user@example.com', fullName: 'John Doe' });
    });
  });

  describe('HTTP Configuration', () => {
    it('should use correct API endpoint for login', (done) => {
      service.login('user@example.com', 'password123').subscribe(
        () => done()
      );

      const req = httpMock.expectOne('/api/auth/login');
      expect(req.request.url).toContain('/api/auth/login');
      req.flush({ success: true, email: 'user@example.com', fullName: 'John Doe' });
    });

    it('should use correct API endpoint for register', (done) => {
      service.register('user@example.com', 'John Doe', 'password123').subscribe(
        () => done()
      );

      const req = httpMock.expectOne('/api/auth/register');
      expect(req.request.url).toContain('/api/auth/register');
      req.flush({ success: true, email: 'user@example.com', fullName: 'John Doe' });
    });

    it('should use correct API endpoint for logout', (done) => {
      service.logout().subscribe(
        () => done()
      );

      const req = httpMock.expectOne('/api/auth/logout');
      expect(req.request.url).toContain('/api/auth/logout');
      req.flush({ success: true });
    });
  });
});
