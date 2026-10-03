import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { ConfirmationComponent } from './confirmation.component';
import { BookingConfirmation } from '../models';

describe('ConfirmationComponent - Seat Number Display', () => {
  let component: ConfirmationComponent;
  let fixture: ComponentFixture<ConfirmationComponent>;
  let router: jasmine.SpyObj<Router>;

  const makeConfirmation = (passengers: { fullName: string; seatNumber: string }[]): BookingConfirmation => ({
    bookingId: 'BK-UUID-001',
    bookingReferenceCode: 'SK20260721ABCDEF',
    flightDetails: {
      airlineName: 'GlobalAir',
      flightNumber: 'GA001',
      origin: 'JFK',
      destination: 'LAX',
      departureTime: '2026-07-21T10:00:00Z',
      arrivalTime: '2026-07-21T15:30:00Z',
      cabinClass: 'Economy'
    },
    pricing: { totalPrice: 230.00, pricePerPassenger: 115.00, numberOfPassengers: passengers.length },
    passengers,
    bookingStatus: 'Confirmed',
    createdAt: '2026-07-20T08:00:00Z'
  });

  beforeEach(async () => {
    router = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [ConfirmationComponent],
      providers: [{ provide: Router, useValue: router }]
    }).compileComponents();

    fixture = TestBed.createComponent(ConfirmationComponent);
    component = fixture.componentInstance;
    // Do NOT call fixture.detectChanges() here — each test controls its own history state
  });

  it('should display each passenger\'s seat number when seats are assigned', () => {
    // Arrange — two passengers with confirmed seats
    const confirmation = makeConfirmation([
      { fullName: 'Alice Smith', seatNumber: '5A' },
      { fullName: 'Bob Jones',   seatNumber: '7C' }
    ]);
    history.pushState({ confirmation }, '');

    // Act — first detectChanges triggers ngOnInit, which reads history.state
    fixture.detectChanges();

    // Assert — both seat numbers appear in the Passengers section
    const el: HTMLElement = fixture.nativeElement;
    expect(el.textContent).toContain('5A');
    expect(el.textContent).toContain('7C');
  });

  it('should render the em-dash fallback when a passenger has no seat number', () => {
    // Arrange — passengers with empty seat numbers (e.g. legacy booking before seat feature)
    const confirmation = makeConfirmation([
      { fullName: 'Alice Smith', seatNumber: '' },
      { fullName: 'Bob Jones',   seatNumber: '' }
    ]);
    history.pushState({ confirmation }, '');

    // Act
    fixture.detectChanges();

    // Assert — template uses `seatNumber || '—'`; empty string falls back to em-dash
    const el: HTMLElement = fixture.nativeElement;
    expect(el.textContent).toContain('—'); // '—'
    expect(el.textContent).not.toContain('Seat: 5A');
    expect(el.textContent).not.toContain('Seat: 7C');
  });
});
