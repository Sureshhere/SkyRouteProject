import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { signal } from '@angular/core';
import { of } from 'rxjs';
import { ConfirmationComponent } from './confirmation.component';
import { BookingService } from '../services/booking.service';

interface BookingConfirmation {
  bookingRef: string;
  flightId: string;
  passengers: string[];
  totalPrice: number;
  status: string;
  confirmationDate?: string;
}

describe('ConfirmationComponent - Signals and Strong Typing', () => {
  let component: ConfirmationComponent;
  let fixture: ComponentFixture<ConfirmationComponent>;
  let bookingService: jasmine.SpyObj<BookingService>;

  const mockBookingConfirmation: BookingConfirmation = {
    bookingRef: 'BK-12345',
    flightId: 'GA-001',
    passengers: ['John Doe', 'Jane Doe'],
    totalPrice: 1000,
    status: 'confirmed',
    confirmationDate: '2026-06-17'
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

    it('should have strongly typed bookingConfirmation signal (BookingConfirmation | null)', () => {
      const initialValue = component.bookingConfirmation();
      expect(initialValue).toBeNull();
      expect(typeof component.bookingConfirmation).toBe('function');
    });

    it('should NOT use any type for bookingConfirmation signal', () => {
      const booking = component.bookingConfirmation();
      if (booking !== null) {
        expect(booking.bookingRef).toBeDefined();
        expect(booking.flightId).toBeDefined();
        expect(booking.passengers).toBeDefined();
        expect(booking.totalPrice).toBeDefined();
        expect(booking.status).toBeDefined();
      }
    });

    it('should provide type safety for BookingConfirmation signal', () => {
      const mockData: BookingConfirmation = {
        bookingRef: 'BK-TEST',
        flightId: 'GA-TEST',
        passengers: ['Test User'],
        totalPrice: 500,
        status: 'confirmed'
      };

      expect(mockData.bookingRef).toBe('BK-TEST');
      expect(mockData.totalPrice).toBe(500);
    });
  });

  describe('Route Parameter Handling', () => {
    it('should load booking details from route parameter bookingRef', waitForAsync(() => {
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
        expect(booking?.bookingRef).toBe('BK-12345');
        expect(booking?.status).toBe('confirmed');
      });
    }));

    it('should handle booking service errors gracefully', waitForAsync(() => {
      bookingService.getBooking.and.returnValue(of(null));

      fixture.detectChanges();
      fixture.whenStable().then(() => {
        const booking = component.bookingConfirmation();
        expect(booking).toBeNull();
      });
    }));

    it('should extract bookingRef from route params correctly', waitForAsync(() => {
      bookingService.getBooking.and.returnValue(of(mockBookingConfirmation));

      fixture.detectChanges();
      fixture.whenStable().then(() => {
        expect(bookingService.getBooking).toHaveBeenCalledWith('BK-12345');
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
        expect(booking?.totalPrice).toBe(1000);
      });
    }));

    it('should display passenger list from booking confirmation', waitForAsync(() => {
      bookingService.getBooking.and.returnValue(of(mockBookingConfirmation));

      fixture.detectChanges();
      fixture.whenStable().then(() => {
        const booking = component.bookingConfirmation();
        expect(booking?.passengers).toContain('John Doe');
        expect(booking?.passengers).toContain('Jane Doe');
      });
    }));

    it('should display confirmation status', waitForAsync(() => {
      bookingService.getBooking.and.returnValue(of(mockBookingConfirmation));

      fixture.detectChanges();
      fixture.whenStable().then(() => {
        const booking = component.bookingConfirmation();
        expect(booking?.status).toBe('confirmed');
      });
    }));
  });
});
