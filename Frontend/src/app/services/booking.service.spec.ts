import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { BookingService } from './booking.service';
import { CreateBookingRequest, BookingConfirmation } from '../models';
import { AuthService } from './auth.service';

describe('BookingService - HttpOnly Cookie Authentication', () => {
  let service: BookingService;
  let httpMock: HttpTestingController;
  let authService: jasmine.SpyObj<AuthService>;

  const mockBookingConfirmation: BookingConfirmation = {
    bookingId: 'BK-UUID-123',
    bookingReferenceCode: 'BK-12345',
    flightDetails: {
      airlineName: 'Global Airways',
      flightNumber: 'GA-123',
      origin: 'NYC',
      destination: 'LAX',
      departureTime: '2026-06-18T10:00:00',
      arrivalTime: '2026-06-18T13:00:00',
      cabinClass: 'Economy'
    },
    pricing: {
      totalPrice: 500,
      pricePerPassenger: 250,
      numberOfPassengers: 2
    },
    bookingStatus: 'CONFIRMED',
    createdAt: '2026-06-17T14:29:00'
  };

  beforeEach(() => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['getToken']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [BookingService, { provide: AuthService, useValue: authServiceSpy }]
    });
    service = TestBed.inject(BookingService);
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    httpMock = TestBed.inject(HttpTestingController);

    authService.getToken.and.returnValue(null);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Create Booking', () => {
    it('should create booking with POST request', (done) => {
      const bookingRequest: CreateBookingRequest = {
        flightId: 'GA-001',
        departureDate: '2026-06-18',
        passengers: [{ email: 'user@example.com', fullName: 'John Doe', documentNumber: 'ABC123', seatNumber: '1A' }]
=======
        passengers: [{ email: 'user@example.com', fullName: 'John Doe', documentNumber: 'ABC123' }]
      };

      service.createBooking(bookingRequest).subscribe({
        next: (response) => {
          expect(response.bookingReferenceCode).toBe('BK-12345');
          done();
        }
      });

      const req = httpMock.expectOne('http://localhost:5235/api/bookings');
      expect(req.request.method).toBe('POST');
      req.flush(mockBookingConfirmation);
    });

    it('should include booking data in request body', (done) => {
      const bookingRequest: CreateBookingRequest = {
        flightId: 'GA-002',
        departureDate: '2026-06-20',
        passengers: [{ email: 'user1@example.com', fullName: 'Jane Smith', documentNumber: 'XYZ789', seatNumber: '1A' }]
=======
        passengers: [{ email: 'user1@example.com', fullName: 'Jane Smith', documentNumber: 'XYZ789' }]
      };

      service.createBooking(bookingRequest).subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne('http://localhost:5235/api/bookings');
      expect(req.request.body).toEqual(bookingRequest);
      req.flush(mockBookingConfirmation);
    });

    it('should handle 400 Bad Request error', (done) => {
      const bookingRequest: CreateBookingRequest = {
        flightId: 'GA-005',
        departureDate: '2026-06-23',
        passengers: [{ email: 'invalid-email', fullName: 'User', documentNumber: 'ABC', seatNumber: '1A' }]
=======
        passengers: [{ email: 'invalid-email', fullName: 'User', documentNumber: 'ABC' }]
      };

      service.createBooking(bookingRequest).subscribe({
        error: (error) => {
          expect(error.status).toBe(400);
          done();
        }
      });

      const req = httpMock.expectOne('http://localhost:5235/api/bookings');
      req.flush({ error: 'Invalid booking request' }, { status: 400, statusText: 'Bad Request' });
    });

    it('should handle 401 Unauthorized error', (done) => {
      const bookingRequest: CreateBookingRequest = {
        flightId: 'GA-006',
        departureDate: '2026-06-24',
        passengers: [{ email: 'user@example.com', fullName: 'User', documentNumber: 'ABC123', seatNumber: '1A' }]
=======
        passengers: [{ email: 'user@example.com', fullName: 'User', documentNumber: 'ABC123' }]
      };

      service.createBooking(bookingRequest).subscribe({
        error: (error) => {
          expect(error.status).toBe(401);
          done();
        }
      });

      const req = httpMock.expectOne('http://localhost:5235/api/bookings');
      req.flush({ error: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });
    });
  });

  describe('Get Booking', () => {
    it('should retrieve booking with GET request', (done) => {
      service.getBooking('BK-12345').subscribe({
        next: (response) => {
          expect(response.bookingReferenceCode).toBe('BK-12345');
          done();
        }
      });

      const req = httpMock.expectOne('http://localhost:5235/api/bookings/BK-12345');
      expect(req.request.method).toBe('GET');
      req.flush(mockBookingConfirmation);
    });

    it('should use correct API endpoint for get booking', (done) => {
      service.getBooking('BK-SPECIFIC').subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne('http://localhost:5235/api/bookings/BK-SPECIFIC');
      expect(req.request.url).toContain('/api/bookings/BK-SPECIFIC');
      req.flush(mockBookingConfirmation);
    });

    it('should handle 404 Not Found error', (done) => {
      service.getBooking('INVALID-REF').subscribe({
        error: (error) => {
          expect(error.status).toBe(404);
          done();
        }
      });

      const req = httpMock.expectOne('http://localhost:5235/api/bookings/INVALID-REF');
      req.flush({ error: 'Booking not found' }, { status: 404, statusText: 'Not Found' });
    });

    it('should handle 401 Unauthorized on get booking', (done) => {
      service.getBooking('BK-12345').subscribe({
        error: (error) => {
          expect(error.status).toBe(401);
          done();
        }
      });

      const req = httpMock.expectOne('http://localhost:5235/api/bookings/BK-12345');
      req.flush({ error: 'Session expired' }, { status: 401, statusText: 'Unauthorized' });
    });
  });

  describe('Cancel Booking', () => {
    it('should cancel booking with DELETE request', (done) => {
      service.cancelBooking('BK-12345').subscribe({
        next: (response) => {
          expect(response.message).toBeDefined();
          done();
        }
      });

      const req = httpMock.expectOne('http://localhost:5235/api/bookings/BK-12345');
      expect(req.request.method).toBe('DELETE');
      req.flush({ message: 'Booking cancelled successfully' });
    });

    it('should use correct API endpoint for cancel booking', (done) => {
      service.cancelBooking('BK-12345').subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne('http://localhost:5235/api/bookings/BK-12345');
      expect(req.request.url).toContain('/api/bookings/BK-12345');
      expect(req.request.method).toBe('DELETE');
      req.flush({ message: 'Cancelled' });
    });

    it('should handle 404 Not Found on cancel', (done) => {
      service.cancelBooking('INVALID-REF').subscribe({
        error: (error) => {
          expect(error.status).toBe(404);
          done();
        }
      });

      const req = httpMock.expectOne('http://localhost:5235/api/bookings/INVALID-REF');
      req.flush({ error: 'Booking not found' }, { status: 404, statusText: 'Not Found' });
    });

    it('should handle 401 Unauthorized on cancel', (done) => {
      service.cancelBooking('BK-12345').subscribe({
        error: (error) => {
          expect(error.status).toBe(401);
          done();
        }
      });

      const req = httpMock.expectOne('http://localhost:5235/api/bookings/BK-12345');
      req.flush({ error: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });
    });
  });

  describe('Document Validation', () => {
    it('should validate national ID format correctly', () => {
      expect(service.validateDocumentNumber('NationalId', 'ABC123DEF')).toBe(true);
      expect(service.validateDocumentNumber('NationalId', 'short')).toBe(false);
    });

    it('should validate passport number format correctly', () => {
      expect(service.validateDocumentNumber('PassportNumber', 'ABC123')).toBe(true);
      expect(service.validateDocumentNumber('PassportNumber', 'AB12')).toBe(false);
    });
  });

  describe('Route Type Detection', () => {
    it('should identify domestic routes correctly', () => {
      expect(service.isDomesticRoute('US', 'US')).toBe(true);
      expect(service.isDomesticRoute('us', 'US')).toBe(true);
    });

    it('should identify international routes correctly', () => {
      expect(service.isDomesticRoute('US', 'CA')).toBe(false);
      expect(service.isDomesticRoute('US', 'UK')).toBe(false);
    });
  });
});
