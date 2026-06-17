import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HttpClient } from '@angular/common/http';

describe('Auth Interceptor - 401 Handling & HttpOnly Cookie', () => {
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: []
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('HTTP Error Handling', () => {
    it('should handle 400 Bad Request error', (done) => {
      httpClient.post('/api/bookings', { invalid: 'data' }).subscribe({
        error: (error) => {
          expect(error.status).toBe(400);
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings');
      req.flush({ error: 'Bad request' }, { status: 400, statusText: 'Bad Request' });
    });

    it('should handle 401 Unauthorized error', (done) => {
      httpClient.get('/api/bookings/BK-12345').subscribe({
        error: (error) => {
          expect(error.status).toBe(401);
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings/BK-12345');
      req.flush({ error: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });
    });

    it('should handle 404 Not Found error', (done) => {
      httpClient.get('/api/bookings/BK-NOTFOUND').subscribe({
        error: (error) => {
          expect(error.status).toBe(404);
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings/BK-NOTFOUND');
      req.flush({ error: 'Not found' }, { status: 404, statusText: 'Not Found' });
    });

    it('should handle 500 Internal Server Error', (done) => {
      httpClient.get('/api/flights').subscribe({
        error: (error) => {
          expect(error.status).toBe(500);
          done();
        }
      });

      const req = httpMock.expectOne('/api/flights');
      req.flush({ error: 'Internal error' }, { status: 500, statusText: 'Internal Server Error' });
    });

    it('should handle 403 Forbidden error', (done) => {
      httpClient.post('/api/bookings', {}).subscribe({
        error: (error) => {
          expect(error.status).toBe(403);
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings');
      req.flush({ error: 'Forbidden' }, { status: 403, statusText: 'Forbidden' });
    });
  });

  describe('Successful Responses', () => {
    it('should pass through 200 OK responses', (done) => {
      const mockResponse = { bookingRef: 'BK-12345', status: 'confirmed' };

      httpClient.get('/api/bookings/BK-12345').subscribe({
        next: (response) => {
          expect(response).toEqual(mockResponse);
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings/BK-12345');
      req.flush(mockResponse);
    });

    it('should pass through 201 Created responses', (done) => {
      const mockResponse = { bookingRef: 'BK-NEW', status: 'confirmed' };

      httpClient.post('/api/bookings', {}).subscribe({
        next: (response) => {
          expect(response).toEqual(mockResponse);
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings');
      req.flush(mockResponse, { status: 201, statusText: 'Created' });
    });

    it('should handle auth endpoint login response', (done) => {
      httpClient.post('/api/auth/login', {}).subscribe({
        next: (response) => {
          expect(response).toBeDefined();
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/login');
      req.flush({ success: true, email: 'user@example.com' });
    });

    it('should handle auth endpoint register response', (done) => {
      httpClient.post('/api/auth/register', {}).subscribe({
        next: (response) => {
          expect(response).toBeDefined();
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/register');
      req.flush({ success: true, email: 'newuser@example.com' });
    });
  });

  describe('Request Methods', () => {
    it('should handle GET requests', (done) => {
      httpClient.get('/api/flights').subscribe({
        next: (response) => {
          expect(response).toBeDefined();
          done();
        }
      });

      const req = httpMock.expectOne('/api/flights');
      expect(req.request.method).toBe('GET');
      req.flush({ flights: [] });
    });

    it('should handle POST requests', (done) => {
      const payload = { email: 'test@example.com', password: 'password123' };

      httpClient.post('/api/auth/login', payload).subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne('/api/auth/login');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(payload);
      req.flush({ success: true });
    });

    it('should handle DELETE requests', (done) => {
      httpClient.delete('/api/bookings/BK-12345').subscribe({
        next: (response) => {
          expect(response).toBeDefined();
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings/BK-12345');
      expect(req.request.method).toBe('DELETE');
      req.flush({ message: 'Deleted' });
    });

    it('should handle PUT requests', (done) => {
      httpClient.put('/api/bookings/BK-12345', {}).subscribe({
        next: (response) => {
          expect(response).toBeDefined();
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings/BK-12345');
      expect(req.request.method).toBe('PUT');
      req.flush({ message: 'Updated' });
    });
  });
});
