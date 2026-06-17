import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { ConfirmationComponent } from '../booking/confirmation.component';
import { BookingConfirmation } from '../models';

describe('ConfirmationComponent - Signals and Strong Typing', () => {
  let component: ConfirmationComponent;
  let fixture: ComponentFixture<ConfirmationComponent>;
  let router: jasmine.SpyObj<Router>;

  const mockBookingConfirmation: BookingConfirmation = {
    bookingId: 'BK-UUID-123',
    bookingReferenceCode: 'BK-12345',
    flightDetails: {
      airlineName: 'Global Airways',
      flightNumber: 'GA-123',
      origin: 'NYC',
      destination: 'LAX',
      departureTime: '2026-06-20T10:00:00Z',
      arrivalTime: '2026-06-20T14:00:00Z',
      cabinClass: 'Economy'
    },
    pricing: {
      totalPrice: 320.00,
      pricePerPassenger: 160.00,
      numberOfPassengers: 2
    },
    bookingStatus: 'CONFIRMED',
    createdAt: '2026-06-17T15:00:00Z'
  };

  beforeEach(async () => {
    router = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [ConfirmationComponent],
      providers: [
        { provide: Router, useValue: router }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ConfirmationComponent);
    component = fixture.componentInstance;
  });

  describe('Signal Initialization', () => {
    it('should initialize confirmation signal as null', () => {
      expect(component.confirmation()).toBeNull();
    });

    it('should have strongly typed confirmation signal', () => {
      component.confirmation.set(mockBookingConfirmation);
      expect(component.confirmation()).toBeDefined();
      expect(component.confirmation()!.bookingReferenceCode).toBe('BK-12345');
    });
  });

  describe('History State Handling', () => {
    it('should load booking details from history state', () => {
      history.pushState({ confirmation: mockBookingConfirmation }, '', '');
      component.ngOnInit();
      expect(component.confirmation()).toEqual(mockBookingConfirmation);
    });

    it('should navigate to flights page when no confirmation in state', () => {
      history.pushState({}, '', '');
      component.ngOnInit();
      expect(router.navigate).toHaveBeenCalledWith(['/flights']);
    });

    it('should populate confirmation signal with correct data', () => {
      history.pushState({ confirmation: mockBookingConfirmation }, '', '');
      component.ngOnInit();
      expect(component.confirmation()!.bookingReferenceCode).toBe('BK-12345');
      expect(component.confirmation()!.flightDetails.origin).toBe('NYC');
      expect(component.confirmation()!.pricing.totalPrice).toBe(320.00);
    });
  });

  describe('Component Display', () => {
    it('should display booking confirmation when signal has data', () => {
      component.confirmation.set(mockBookingConfirmation);
      fixture.detectChanges();
      const compiled = fixture.nativeElement;
      expect(compiled.textContent).toContain('BK-12345');
    });

    it('should display total price from booking confirmation', () => {
      component.confirmation.set(mockBookingConfirmation);
      fixture.detectChanges();
      const compiled = fixture.nativeElement;
      expect(compiled.textContent).toContain('320.00');
    });

    it('should display flight details', () => {
      component.confirmation.set(mockBookingConfirmation);
      fixture.detectChanges();
      const compiled = fixture.nativeElement;
      expect(compiled.textContent).toContain('GA-123');
      expect(compiled.textContent).toContain('Economy');
    });

    it('should display confirmation status badge', () => {
      component.confirmation.set(mockBookingConfirmation);
      fixture.detectChanges();
      const compiled = fixture.nativeElement;
      expect(compiled.textContent).toContain('CONFIRMED');
    });
  });

  describe('Navigation', () => {
    it('should navigate to flights on goHome', () => {
      component.goHome();
      expect(router.navigate).toHaveBeenCalledWith(['/flights']);
    });

    it('should navigate to flights on searchMore', () => {
      component.searchMore();
      expect(router.navigate).toHaveBeenCalledWith(['/flights']);
    });
  });

  describe('Price Formatting', () => {
    it('should format price with USD currency and 2 decimals', () => {
      const formatted = component.formatPrice(160.00);
      expect(formatted).toBe('USD 160.00');
    });

    it('should format price with decimals for fractional amounts', () => {
      const formatted = component.formatPrice(99.99);
      expect(formatted).toBe('USD 99.99');
    });

    it('should format whole numbers with .00', () => {
      const formatted = component.formatPrice(250);
      expect(formatted).toBe('USD 250.00');
    });
  });
});
