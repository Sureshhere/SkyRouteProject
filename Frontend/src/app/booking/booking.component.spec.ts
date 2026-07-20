import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';

import { BookingComponent } from './booking.component';
import { BookingService } from '../services/booking.service';
import { FlightService } from '../services/flight.service';
import { FlightResult, SeatAvailability } from '../models';

describe('BookingComponent - Seat Selection', () => {
  let component: BookingComponent;
  let fixture: ComponentFixture<BookingComponent>;
  let bookingService: jasmine.SpyObj<BookingService>;
  let flightService: jasmine.SpyObj<FlightService>;
  let router: jasmine.SpyObj<Router>;

  // A JFK→LAX flight (both US) — isDomestic() returns true, National ID required
  const mockFlight: FlightResult = {
    id: 'flight-uuid-001',
    airlineName: 'GlobalAir',
    airlineCode: 'GA',
    flightNumber: 'GA001',
    originCode: 'JFK',
    destinationCode: 'LAX',
    departureTime: '2026-07-21T10:00:00Z',
    arrivalTime: '2026-07-21T15:30:00Z',
    durationMinutes: 330,
    cabinClass: 'Economy',
    pricing: { baseFare: 100, pricePerPassenger: 115, totalPrice: 230, pricingRule: 'GlobalAir +15%' }
  };

  const mockSeatAvailability: SeatAvailability = {
    flightId: '00000001-0000-0000-0000-000000000000',
    departureDate: '2026-07-21',
    cabinClass: 'Economy',
    availableSeats: ['1A', '1B', '1C', '2A', '2B', '2C']
  };

  beforeEach(async () => {
    bookingService = jasmine.createSpyObj('BookingService', ['createBooking']);
    flightService = jasmine.createSpyObj('FlightService', ['getAvailableSeats', 'formatTime']);
    router = jasmine.createSpyObj('Router', ['navigate']);

    flightService.getAvailableSeats.and.returnValue(of(mockSeatAvailability));
    flightService.formatTime.and.returnValue('10:00 AM');

    // Push history state BEFORE detectChanges so ngOnInit can read it on its
    // first lifecycle call and initialize the form with 2 passengers.
    history.pushState(
      { flight: mockFlight, search: { departureDate: '2026-07-21', numberOfPassengers: 2 } },
      ''
    );

    await TestBed.configureTestingModule({
      imports: [BookingComponent],
      providers: [
        { provide: BookingService, useValue: bookingService },
        { provide: FlightService, useValue: flightService },
        { provide: Router, useValue: router }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BookingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges(); // triggers ngOnInit → initializes form and loads mock seats
  });

  // ── getAvailableSeatsFor ───────────────────────────────────────────────────

  describe('getAvailableSeatsFor', () => {
    it('should exclude the seat already chosen by passenger 1 from passenger 0\'s options', () => {
      // Arrange — passenger 1 selects seat 1B
      component.passengersArray.at(1).get('seatNumber')!.setValue('1B');

      // Act
      const seatsForPassenger0 = component.getAvailableSeatsFor(0);

      // Assert — 1B is reserved for passenger 1 and must not appear for passenger 0
      expect(seatsForPassenger0).not.toContain('1B');
      expect(seatsForPassenger0).toContain('1A');
      expect(seatsForPassenger0).toContain('1C');
    });

    it('should exclude the seat already chosen by passenger 0 from passenger 1\'s options', () => {
      // Arrange — passenger 0 selects seat 2A
      component.passengersArray.at(0).get('seatNumber')!.setValue('2A');

      // Act
      const seatsForPassenger1 = component.getAvailableSeatsFor(1);

      // Assert — 2A is reserved for passenger 0 and must not appear for passenger 1
      expect(seatsForPassenger1).not.toContain('2A');
      expect(seatsForPassenger1).toContain('2B');
      expect(seatsForPassenger1).toContain('2C');
    });
  });

  // ── Submit button enabled / disabled ─────────────────────────────────────

  describe('submit button state', () => {
    it('should be disabled when a passenger has no seat selected', () => {
      // Arrange — passenger 0 is fully valid but passenger 1 has no seat
      component.passengersArray.at(0).patchValue({
        fullName: 'Alice Smith',
        email: 'alice@example.com',
        documentNumber: 'AB12345678',  // 10 chars — valid National ID for domestic route
        seatNumber: '1A'
      });
      component.passengersArray.at(1).patchValue({
        fullName: 'Bob Jones',
        email: 'bob@example.com',
        documentNumber: 'CD87654321',
        seatNumber: ''  // no seat selected → seatNumber is required → form invalid
      });
      fixture.detectChanges();

      // Act
      const submitButton: HTMLButtonElement = fixture.nativeElement.querySelector('button[type="submit"]');

      // Assert
      expect(submitButton.disabled).toBeTrue();
    });

    it('should be enabled when all passengers have valid details and seats selected', () => {
      // Arrange
      component.passengersArray.at(0).patchValue({
        fullName: 'Alice Smith',
        email: 'alice@example.com',
        documentNumber: 'AB12345678',
        seatNumber: '1A'
      });
      component.passengersArray.at(1).patchValue({
        fullName: 'Bob Jones',
        email: 'bob@example.com',
        documentNumber: 'CD87654321',
        seatNumber: '1B'
      });
      fixture.detectChanges();

      // Act
      const submitButton: HTMLButtonElement = fixture.nativeElement.querySelector('button[type="submit"]');

      // Assert
      expect(submitButton.disabled).toBeFalse();
    });
  });
});
