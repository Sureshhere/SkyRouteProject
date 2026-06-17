import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

describe('Auth Interceptor - 401 Handling & HttpOnly Cookie', () => {
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['logout']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('401 Error Handling - Non-Auth Endpoints', () => {
    it('should call logout on 401 response from /api/bookings endpoint', (done) => {
      httpClient.get('/api/bookings/BK-12345').subscribe({
        error: () => {
          expect(authService.logout).toHaveBeenCalled();
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings/BK-12345');
      req.flush({ error: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });
    });

    it('should navigate to /login after logout on 401', (done) => {
      httpClient.get('/api/flights').subscribe({
        error: () => {
          expect(router.navigate).toHaveBeenCalledWith(['/login']);
          done();
        }
      });

      const req = httpMock.expectOne('/api/flights');
      req.flush({ error: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });
    });

    it('should handle 401 from /api/bookings/create endpoint', (done) => {
      httpClient.post('/api/bookings', {}).subscribe({
        error: () => {
          expect(authService.logout).toHaveBeenCalled();
          expect(router.navigate).toHaveBeenCalledWith(['/login']);
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings');
      req.flush({ error: 'Session expired' }, { status: 401, statusText: 'Unauthorized' });
    });

    it('should handle 401 from /api/flights endpoint', (done) => {
      httpClient.get('/api/flights?departure=NYC').subscribe({
        error: () => {
          expect(authService.logout).toHaveBeenCalled();
          done();
        }
      });

      const req = httpMock.expectOne('/api/flights?departure=NYC');
      req.flush({ error: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });
    });
  });

  describe('401 Error Handling - Auth Endpoints (Recursion Prevention)', () => {
    it('should NOT call logout on 401 from /api/auth/login endpoint', (done) => {
      httpClient.post('/api/auth/login', { email: 'user@example.com', password: 'wrong' }).subscribe({
        error: () => {
          expect(authService.logout).not.toHaveBeenCalled();
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/login');
      req.flush({ error: 'Invalid credentials' }, { status: 401, statusText: 'Unauthorized' });
    });

    it('should NOT call logout on 401 from /api/auth/register endpoint', (done) => {
      httpClient.post('/api/auth/register', {}).subscribe({
        error: () => {
          expect(authService.logout).not.toHaveBeenCalled();
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/register');
      req.flush({ error: 'Email exists' }, { status: 401, statusText: 'Unauthorized' });
    });

    it('should NOT call logout on 401 from /api/auth/logout endpoint', (done) => {
      httpClient.post('/api/auth/logout', {}).subscribe({
        error: () => {
          expect(authService.logout).not.toHaveBeenCalled();
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/logout');
      req.flush({ error: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });
    });

    it('should prevent infinite logout loop on 401 from auth endpoints', (done) => {
      httpClient.post('/api/auth/login', {}).subscribe({
        error: () => {
          expect(authService.logout).not.toHaveBeenCalled();
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/login');
      req.flush({ error: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });
    });
  });

  describe('Non-401 Error Handling', () => {
    it('should pass through 400 errors unchanged', (done) => {
      httpClient.post('/api/bookings', { invalid: 'data' }).subscribe({
        error: (error) => {
          expect(error.status).toBe(400);
          expect(authService.logout).not.toHaveBeenCalled();
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings');
      req.flush({ error: 'Bad request' }, { status: 400, statusText: 'Bad Request' });
    });

    it('should pass through 404 errors unchanged', (done) => {
      httpClient.get('/api/bookings/BK-NOTFOUND').subscribe({
        error: (error) => {
          expect(error.status).toBe(404);
          expect(authService.logout).not.toHaveBeenCalled();
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings/BK-NOTFOUND');
      req.flush({ error: 'Not found' }, { status: 404, statusText: 'Not Found' });
    });

    it('should pass through 500 errors unchanged', (done) => {
      httpClient.get('/api/flights').subscribe({
        error: (error) => {
          expect(error.status).toBe(500);
          expect(authService.logout).not.toHaveBeenCalled();
          done();
        }
      });

      const req = httpMock.expectOne('/api/flights');
      req.flush({ error: 'Internal error' }, { status: 500, statusText: 'Internal Server Error' });
    });

    it('should pass through 403 (Forbidden) errors unchanged', (done) => {
      httpClient.post('/api/bookings', {}).subscribe({
        error: (error) => {
          expect(error.status).toBe(403);
          expect(authService.logout).not.toHaveBeenCalled();
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings');
      req.flush({ error: 'Forbidden' }, { status: 403, statusText: 'Forbidden' });
    });
  });

  describe('Successful Responses', () => {
    it('should pass through 200 OK responses unchanged', (done) => {
      const mockResponse = { bookingRef: 'BK-12345', status: 'confirmed' };

      httpClient.get('/api/bookings/BK-12345').subscribe({
        next: (response) => {
          expect(response).toEqual(mockResponse);
          expect(authService.logout).not.toHaveBeenCalled();
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings/BK-12345');
      req.flush(mockResponse);
    });

    it('should pass through 201 Created responses unchanged', (done) => {
      const mockResponse = { bookingRef: 'BK-NEW', status: 'confirmed' };

      httpClient.post('/api/bookings', {}).subscribe({
        next: (response) => {
          expect(response).toEqual(mockResponse);
          expect(authService.logout).not.toHaveBeenCalled();
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings');
      req.flush(mockResponse, { status: 201, statusText: 'Created' });
    });

    it('should not trigger logout on successful auth endpoint responses', (done) => {
      httpClient.post('/api/auth/login', {}).subscribe({
        next: () => {
          expect(authService.logout).not.toHaveBeenCalled();
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/login');
      req.flush({ success: true, email: 'user@example.com' });
    });
  });
});
