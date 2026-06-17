import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { signal } from '@angular/core';
import { of } from 'rxjs';
import { ConfirmationComponent } from './confirmation.component';
import { BookingService } from '../services/booking.service';
import { BookingConfirmation } from '../models';

describe('ConfirmationComponent - Signals and Strong Typing', () => {
  let component: ConfirmationComponent;
  let fixture: ComponentFixture<ConfirmationComponent>;
  let bookingService: jasmine.SpyObj<BookingService>;

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
      totalPrice: 1000,
      pricePerPassenger: 500,
      numberOfPassengers: 2
    },
    bookingStatus: 'CONFIRMED',
    createdAt: '2026-06-17'
  };

  beforeEach(waitForAsync(() => {
    const bookingServiceSpy = jasmine.createSpyObj('BookingService', ['getBooking']);

    TestBed.configureTestingModule({
      declarations: [ConfirmationComponent],
      providers: [
        { provide: BookingService, useValue: bookingServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            params: of({ bookingRef: 'BK-12345' })
          }
        }
      ]
    }).compileComponents();

    bookingService = TestBed.inject(BookingService) as jasmine.SpyObj<BookingService>;
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ConfirmationComponent);
    component = fixture.componentInstance;
  });

  describe('Signal Initialization', () => {
    it('should initialize bookingConfirmation signal as null', () => {
      expect(component.bookingConfirmation()).toBeNull();
    });

    it('should have strongly typed bookingConfirmation signal', () => {
      const initialValue = component.bookingConfirmation();
      expect(initialValue).toBeNull();
      expect(typeof component.bookingConfirmation).toBe('function');
    });
  });

  describe('Route Parameter Handling', () => {
    it('should load booking details from route parameter', waitForAsync(() => {
      bookingService.getBooking.and.returnValue(of(mockBookingConfirmation));

      fixture.detectChanges();
      fixture.whenStable().then(() => {
        expect(bookingService.getBooking).toHaveBeenCalledWith('BK-12345');
      });
    }));

    it('should populate bookingConfirmation signal with fetched data', waitForAsync(() => {
      bookingService.getBooking.and.returnValue(of(mockBookingConfirmation));

      fixture.detectChanges();
      fixture.whenStable().then(() => {
        const booking = component.bookingConfirmation();
        expect(booking).toEqual(mockBookingConfirmation);
        expect(booking?.bookingReferenceCode).toBe('BK-12345');
        expect(booking?.bookingStatus).toBe('CONFIRMED');
      });
    }));

    it('should handle booking service errors gracefully', waitForAsync(() => {
      bookingService.getBooking.and.returnValue(of(null as any));

      fixture.detectChanges();
      fixture.whenStable().then(() => {
        const booking = component.bookingConfirmation();
        expect(booking).toBeNull();
      });
    }));
  });

  describe('Component Display and Template', () => {
    it('should display booking confirmation details when signal has data', waitForAsync(() => {
      bookingService.getBooking.and.returnValue(of(mockBookingConfirmation));

      fixture.detectChanges();
      fixture.whenStable().then(() => {
        fixture.detectChanges();
        const compiled = fixture.nativeElement;
        expect(compiled.textContent).toContain('BK-12345');
      });
    }));

    it('should display total price from booking confirmation', waitForAsync(() => {
      bookingService.getBooking.and.returnValue(of(mockBookingConfirmation));

      fixture.detectChanges();
      fixture.whenStable().then(() => {
        const booking = component.bookingConfirmation();
        expect(booking?.pricing.totalPrice).toBe(1000);
      });
    }));

    it('should display flight details from booking confirmation', waitForAsync(() => {
      bookingService.getBooking.and.returnValue(of(mockBookingConfirmation));

      fixture.detectChanges();
      fixture.whenStable().then(() => {
        const booking = component.bookingConfirmation();
        expect(booking?.flightDetails.airlineName).toBe('Global Airways');
        expect(booking?.flightDetails.flightNumber).toBe('GA-123');
      });
    }));

    it('should display confirmation status', waitForAsync(() => {
      bookingService.getBooking.and.returnValue(of(mockBookingConfirmation));

      fixture.detectChanges();
      fixture.whenStable().then(() => {
        const booking = component.bookingConfirmation();
        expect(booking?.bookingStatus).toBe('CONFIRMED');
      });
    }));
  });
});
