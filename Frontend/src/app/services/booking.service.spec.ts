import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { BookingService } from './booking.service';

describe('BookingService - HttpOnly Cookie Authentication', () => {
  let service: BookingService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [BookingService]
    });
    service = TestBed.inject(BookingService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Create Booking', () => {
    it('should create booking with withCredentials: true', (done) => {
      const bookingRequest = {
        flightId: 'GA-001',
        passengers: [{ email: 'user@example.com', fullName: 'John Doe', documentNumber: 'ABC123' }],
        totalPrice: 500
      };

      service.createBooking(bookingRequest).subscribe({
        next: (response) => {
          expect(response.bookingRef).toBe('BK-12345');
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings');
      expect(req.request.method).toBe('POST');
      expect(req.request.withCredentials).toBe(true);
      req.flush({ bookingRef: 'BK-12345', status: 'confirmed', totalPrice: 500 });
    });

    it('should send booking data correctly in request body', (done) => {
      const bookingRequest = {
        flightId: 'GA-002',
        passengers: [{ email: 'user1@example.com', fullName: 'Jane Smith', documentNumber: 'XYZ789' }],
        totalPrice: 750
      };

      service.createBooking(bookingRequest).subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings');
      expect(req.request.body).toEqual(bookingRequest);
      req.flush({ bookingRef: 'BK-12346', status: 'confirmed', totalPrice: 750 });
    });

    it('should handle multiple passengers in booking', (done) => {
      const bookingRequest = {
        flightId: 'GA-003',
        passengers: [
          { email: 'user1@example.com', fullName: 'John Doe', documentNumber: 'ABC123' },
          { email: 'user2@example.com', fullName: 'Jane Doe', documentNumber: 'DEF456' }
        ],
        totalPrice: 1000
      };

      service.createBooking(bookingRequest).subscribe({
        next: (response) => {
          expect(response.bookingRef).toBeDefined();
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings');
      expect(req.request.body.passengers.length).toBe(2);
      req.flush({ bookingRef: 'BK-12347', status: 'confirmed', totalPrice: 1000 });
    });

    it('should handle server errors on create booking', (done) => {
      const bookingRequest = {
        flightId: 'INVALID',
        passengers: [{ email: 'user@example.com', fullName: 'John Doe', documentNumber: 'ABC123' }],
        totalPrice: 500
      };

      service.createBooking(bookingRequest).subscribe({
        error: (error) => {
          expect(error.status).toBe(400);
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings');
      req.flush({ error: 'Invalid flight ID' }, { status: 400, statusText: 'Bad Request' });
    });

    it('should handle authentication errors on create booking', (done) => {
      const bookingRequest = {
        flightId: 'GA-001',
        passengers: [{ email: 'user@example.com', fullName: 'John Doe', documentNumber: 'ABC123' }],
        totalPrice: 500
      };

      service.createBooking(bookingRequest).subscribe({
        error: (error) => {
          expect(error.status).toBe(401);
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings');
      req.flush({ error: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });
    });
  });

  describe('Get Booking', () => {
    it('should get booking with withCredentials: true', (done) => {
      const bookingRef = 'BK-12345';

      service.getBooking(bookingRef).subscribe({
        next: (booking) => {
          expect(booking.bookingRef).toBe('BK-12345');
          done();
        }
      });

      const req = httpMock.expectOne(`/api/bookings/${bookingRef}`);
      expect(req.request.method).toBe('GET');
      expect(req.request.withCredentials).toBe(true);
      req.flush({ bookingRef: 'BK-12345', status: 'confirmed', totalPrice: 500 });
    });

    it('should construct correct URL for get booking request', (done) => {
      const bookingRef = 'BK-99999';

      service.getBooking(bookingRef).subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne(`/api/bookings/${bookingRef}`);
      expect(req.request.url).toContain(`/api/bookings/BK-99999`);
      req.flush({ bookingRef: 'BK-99999', status: 'confirmed', totalPrice: 500 });
    });

    it('should handle booking not found error', (done) => {
      const bookingRef = 'BK-NOTFOUND';

      service.getBooking(bookingRef).subscribe({
        error: (error) => {
          expect(error.status).toBe(404);
          done();
        }
      });

      const req = httpMock.expectOne(`/api/bookings/${bookingRef}`);
      req.flush({ error: 'Booking not found' }, { status: 404, statusText: 'Not Found' });
    });

    it('should handle authentication error on get booking', (done) => {
      service.getBooking('BK-12345').subscribe({
        error: (error) => {
          expect(error.status).toBe(401);
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings/BK-12345');
      req.flush({ error: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });
    });

    it('should return booking details correctly', (done) => {
      const mockBooking = {
        bookingRef: 'BK-12345',
        flightId: 'GA-001',
        passengers: ['John Doe'],
        totalPrice: 500,
        status: 'confirmed'
      };

      service.getBooking('BK-12345').subscribe({
        next: (booking) => {
          expect(booking.bookingRef).toBe('BK-12345');
          expect(booking.flightId).toBe('GA-001');
          expect(booking.status).toBe('confirmed');
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings/BK-12345');
      req.flush(mockBooking);
    });
  });

  describe('Cancel Booking', () => {
    it('should cancel booking with withCredentials: true', (done) => {
      const bookingRef = 'BK-12345';

      service.cancelBooking(bookingRef).subscribe({
        next: (response) => {
          expect(response.status).toBe('cancelled');
          done();
        }
      });

      const req = httpMock.expectOne(`/api/bookings/${bookingRef}/cancel`);
      expect(req.request.method).toBe('POST');
      expect(req.request.withCredentials).toBe(true);
      req.flush({ bookingRef: 'BK-12345', status: 'cancelled' });
    });

    it('should send cancel request without manual auth headers', (done) => {
      service.cancelBooking('BK-12345').subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings/BK-12345/cancel');
      expect(req.request.headers.has('Authorization')).toBe(false);
      req.flush({ bookingRef: 'BK-12345', status: 'cancelled' });
    });

    it('should handle authentication errors on cancel booking', (done) => {
      service.cancelBooking('BK-12345').subscribe({
        error: (error) => {
          expect(error.status).toBe(401);
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings/BK-12345/cancel');
      req.flush({ error: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });
    });

    it('should handle booking not found on cancel', (done) => {
      service.cancelBooking('BK-NOTFOUND').subscribe({
        error: (error) => {
          expect(error.status).toBe(404);
          done();
        }
      });

      const req = httpMock.expectOne('/api/bookings/BK-NOTFOUND/cancel');
      req.flush({ error: 'Booking not found' }, { status: 404, statusText: 'Not Found' });
    });
  });

  describe('HTTP Configuration', () => {
    it('should use correct API endpoint for create booking', (done) => {
      const bookingRequest = {
        flightId: 'GA-001',
        passengers: [{ email: 'user@example.com', fullName: 'John Doe', documentNumber: 'ABC123' }],
        totalPrice: 500
      };

      service.createBooking(bookingRequest).subscribe(() => done());

      const req = httpMock.expectOne('/api/bookings');
      expect(req.request.url).toContain('/api/bookings');
      req.flush({ bookingRef: 'BK-12345', status: 'confirmed', totalPrice: 500 });
    });

    it('should use correct API endpoint for get booking', (done) => {
      service.getBooking('BK-12345').subscribe(() => done());

      const req = httpMock.expectOne('/api/bookings/BK-12345');
      expect(req.request.url).toContain('/api/bookings/BK-12345');
      req.flush({ bookingRef: 'BK-12345', status: 'confirmed', totalPrice: 500 });
    });

    it('should use correct API endpoint for cancel booking', (done) => {
      service.cancelBooking('BK-12345').subscribe(() => done());

      const req = httpMock.expectOne('/api/bookings/BK-12345/cancel');
      expect(req.request.url).toContain('/api/bookings/BK-12345/cancel');
      req.flush({ bookingRef: 'BK-12345', status: 'cancelled' });
    });
  });
});
