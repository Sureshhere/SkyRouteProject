import { Component, signal, computed, OnInit } from '@angular/core';
=======
﻿import { Component, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { BookingService } from '../services/booking.service';
import { FlightService } from '../services/flight.service';
import { FlightResult } from '../models';

// Hardcoded airport code → country code mapping (matches seeded airport data)
const AIRPORT_COUNTRY: Record<string, string> = {
  JFK: 'US', LAX: 'US', ORD: 'US',
  LHR: 'GB',
  BOM: 'IN', DEL: 'IN'
};

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="booking-page">
      <h2 style="font-size: 22px; font-weight: 700; margin-bottom: 20px; color: var(--text);">Complete Your Booking</h2>

      <div class="alert alert-error" *ngIf="error()">
        {{ error() }}
      </div>

      <div class="booking-flight-summary" *ngIf="flight()">
        <div class="booking-flight-info">
          <div class="b-airline">{{ flight()!.airlineName }} · {{ flight()!.flightNumber }}</div>
          <div class="b-route">
            {{ flight()!.originCode }} {{ flightService.formatTime(flight()!.departureTime) }}
            → {{ flight()!.destinationCode }} {{ flightService.formatTime(flight()!.arrivalTime) }}
            · {{ flight()!.cabinClass }} · {{ getDuration(flight()!.durationMinutes) }}
          </div>
        </div>
        <div class="booking-price-box">
          <div class="b-price">USD {{ flight()!.pricing.totalPrice.toFixed(2) }}</div>
          <div class="b-price-small">USD {{ flight()!.pricing.pricePerPassenger.toFixed(2) }} / person</div>
        </div>
      </div>

      <div class="alert alert-info" *ngIf="flight()" style="margin-bottom: 20px;">
        <strong>{{ isDomestic() ? 'Domestic Flight' : 'International Flight' }}</strong>
        — {{ isDomestic() ? 'National ID required for each passenger' : 'Passport Number required for each passenger' }}
      </div>

      <form [formGroup]="form" (ngSubmit)="submitBooking()" *ngIf="form">
        <div class="passenger-section">
          <div class="passenger-heading">Passenger Details</div>

          <div formArrayName="passengers">
            <div *ngFor="let passenger of passengersArray.controls; let i = index"
                 [formGroupName]="i">
              <div class="passenger-tag">Passenger {{ i + 1 }}</div>

              <div class="form-group">
                <label>Full Name</label>
                <input type="text" formControlName="fullName" placeholder="e.g. John Doe">
                <div class="error" *ngIf="passenger.get('fullName')?.hasError('required') && passenger.get('fullName')?.touched">
                  Full name is required
                </div>
              </div>

              <div class="form-group">
                <label>Email Address</label>
                <input type="email" formControlName="email" placeholder="e.g. john@example.com">
                <div class="error" *ngIf="passenger.get('email')?.hasError('required') && passenger.get('email')?.touched">
                  Email address is required
                </div>
                <div class="error" *ngIf="passenger.get('email')?.hasError('email') && passenger.get('email')?.touched">
                  Please enter a valid email address (e.g. john@example.com)
                </div>
              </div>

              <div class="form-group">
                <label>{{ isDomestic() ? 'National ID' : 'Passport Number' }}</label>
                <input type="text" formControlName="documentNumber"
                       [placeholder]="isDomestic() ? 'e.g. AB12345678 (8–12 characters)' : 'e.g. AB123456 (6–9 characters)'">
                <div class="error" *ngIf="passenger.get('documentNumber')?.hasError('required') && passenger.get('documentNumber')?.touched">
                  {{ isDomestic() ? 'National ID is required' : 'Passport Number is required' }}
                </div>
                <div class="error" *ngIf="passenger.get('documentNumber')?.hasError('pattern') && passenger.get('documentNumber')?.touched">
                  {{ isDomestic() ? 'National ID must be 8–12 alphanumeric characters' : 'Passport Number must be 6–9 alphanumeric characters' }}
                </div>
              </div>

              <div *ngIf="seatsLoading()" style="padding: 8px 0; color: #666; font-size: 13px;">
                Loading available seats...
              </div>

              <div *ngIf="seatsError() && i === 0" class="alert alert-error" style="display: flex; align-items: center; gap: 8px;">
                ⚠ {{ seatsError() }}
                <button type="button" class="btn-secondary" (click)="loadAvailableSeats()" style="padding: 6px 14px; font-size: 13px; flex-shrink: 0; margin-left: 8px;">Retry</button>
              </div>

              <div *ngIf="seatsLoaded() && availableSeats().length === 0 && !seatsError() && i === 0" class="alert alert-info">
                No seats available for this flight.
              </div>

              <div class="form-group">
                <label>Select Seat</label>
                <select formControlName="seatNumber">
                  <option value="">-- Select a seat --</option>
                  <option *ngFor="let seat of getAvailableSeatsFor(i)" [value]="seat">{{ seat }}</option>
                </select>
                <div class="error" *ngIf="passenger.get('seatNumber')?.hasError('required') && passenger.get('seatNumber')?.touched">
                  Please select a seat
=======
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="booking-actions">
          <button type="submit" class="btn-confirm" [disabled]="!form.valid || loading() || seatsLoading()">
            {{ loading() ? 'Processing...' : 'Confirm Booking' }}
          </button>
          <button type="button" class="btn-secondary" (click)="goBack()" style="padding: 13px 24px;">
            ← Back
          </button>
        </div>
      </form>
    </div>
  `
})
export class BookingComponent implements OnInit {
  form!: FormGroup;
  flight = signal<FlightResult | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);
  departureDate = '';
  availableSeats = signal<string[]>([]);
  seatsLoading = signal(false);
  seatsError = signal<string | null>(null);
  seatsLoaded = signal(false);

  isDomestic = computed(() => {
    const f = this.flight();
    if (!f) return false;
    const originCountry = AIRPORT_COUNTRY[f.originCode.toUpperCase()];
    const destCountry = AIRPORT_COUNTRY[f.destinationCode.toUpperCase()];
    return !!originCountry && !!destCountry && originCountry === destCountry;
  });

  isDomestic = computed(() => {
    const f = this.flight();
    if (!f) return false;
    const originCountry = AIRPORT_COUNTRY[f.originCode.toUpperCase()];
    const destCountry = AIRPORT_COUNTRY[f.destinationCode.toUpperCase()];
    return !!originCountry && !!destCountry && originCountry === destCountry;
  });

  get passengersArray() {
    return this.form?.get('passengers') as FormArray;
  }

  constructor(
    private fb: FormBuilder,
    private bookingService: BookingService,
    public flightService: FlightService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const state = history.state;
    if (!state?.flight) {
      this.router.navigate(['/flights']);
      return;
    }

    const flight = state.flight as FlightResult;
    const search = state.search;
    this.flight.set(flight);
    this.departureDate = search?.departureDate || new Date().toISOString().split('T')[0];
    const numPassengers = search?.numberOfPassengers || 1;
    this.initializeForm(numPassengers);
    this.loadAvailableSeats();
  }

  private initializeForm(numPassengers: number): void {
    this.form = this.fb.group({
      passengers: this.fb.array(
        Array.from({ length: numPassengers }, () => this.createPassengerGroup())
      )
    });
  }

  loadAvailableSeats(): void {
    const f = this.flight();
    if (!f) return;
    this.seatsLoading.set(true);
    this.seatsError.set(null);
    this.flightService.getAvailableSeats(f.id, this.departureDate).subscribe({
      next: (result) => {
        this.availableSeats.set(result.availableSeats);
        this.seatsLoading.set(false);
        this.seatsLoaded.set(true);
      },
      error: (err: HttpErrorResponse) => {
        this.seatsLoading.set(false);
        this.seatsLoaded.set(true);
        this.seatsError.set(err.status === 404
          ? 'Flight not found. Please go back and select a different flight.'
          : 'Unable to load available seats. Please try again.');
      }
    });
  }

  private createPassengerGroup(): FormGroup {
    // Pattern matches backend rules: National ID 8–12, Passport 6–9 alphanumeric
    const pattern = this.isDomestic()
      ? /^[A-Za-z0-9]{8,12}$/
      : /^[A-Za-z0-9]{6,9}$/;

    return this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      documentNumber: ['', [Validators.required, Validators.pattern(pattern)]],
      seatNumber: ['', Validators.required]
=======
      documentNumber: ['', [Validators.required, Validators.pattern(pattern)]]
    });
  }

  getOtherSelectedSeats(passengerIndex: number): string[] {
    return this.passengersArray.controls
      .filter((_, i) => i !== passengerIndex)
      .map(c => c.get('seatNumber')?.value as string)
      .filter(s => !!s);
  }

  getAvailableSeatsFor(passengerIndex: number): string[] {
    const others = this.getOtherSelectedSeats(passengerIndex);
    return this.availableSeats().filter(s => !others.includes(s));
  }

  submitBooking(): void {
    if (!this.form.valid || !this.flight()) return;

    this.loading.set(true);
    this.error.set(null);

    const booking = {
      flightId: this.flight()!.id,
      departureDate: this.departureDate,
      passengers: this.passengersArray.value
    };

    this.bookingService.createBooking(booking).subscribe({
      next: (confirmation) => {
        this.router.navigate(['/confirmation'], { state: { confirmation } });
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        if (err.status === 401) {
          this.error.set('Your session has expired. Please log in again to continue.');
        } else if (err.status === 409) {
          this.error.set(err.error?.error || 'One or more selected seats are no longer available. Please select different seats.');
        } else if (Array.isArray(err.error?.errors) && err.error.errors.length > 0) {
          this.error.set(err.error.errors.join(' '));
        } else {
          this.error.set(err.error?.error || err.error?.message || 'Booking failed. Please try again.');
        }
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/results']);
  }

  getDuration(minutes: number): string {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return `${hours}h ${mins}m`;
  }
}
