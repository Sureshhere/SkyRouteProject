import { Component, signal, OnInit, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { Router } from '@angular/router';
import { BookingService } from '../../services/booking.service';
import { FlightService } from '../../services/flight.service';
import { FlightResult, Airport } from '../../models';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="container">
      <div class="page-container">
        <h2>Complete Your Booking</h2>

        <div class="alert alert-error" *ngIf="error()">
          {{ error() }}
        </div>

        <div class="card" *ngIf="flight()" style="margin-bottom: 30px; background: #f9fafb;">
          <h3 style="margin-top: 0;">Flight Summary</h3>
          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
            <div>
              <strong>{{ flight()!.airline }}</strong><br>
              {{ flightService.formatTime(flight()!.departureTime) }} - {{ flightService.formatTime(flight()!.arrivalTime) }}
            </div>
            <div style="text-align: right;">
              <strong style="font-size: 18px; color: #1e3a8a;">{{ flightService.formatPrice(flight()!.totalPrice) }}</strong><br>
              <small>{{ flightService.formatPrice(flight()!.pricePerPassenger) }}/person</small>
            </div>
          </div>
        </div>

        <form [formGroup]="form" (ngSubmit)="submitBooking()" *ngIf="form">
          <div class="card">
            <h3>Passenger Details</h3>

            <div formArrayName="passengers" *ngIf="passengersArray">
              <div *ngFor="let passenger of passengersArray.controls; let i = index" 
                   [formGroupName]="i"
                   style="margin-bottom: 30px; padding-bottom: 20px; border-bottom: 1px solid #e5e7eb;">
                <h4>Passenger {{ i + 1 }}</h4>

                <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
                  <div class="form-group">
                    <label>First Name</label>
                    <input type="text" formControlName="firstName" placeholder="John">
                    <div class="error" *ngIf="passenger.get('firstName')?.hasError('required') && passenger.get('firstName')?.touched">
                      Required
                    </div>
                  </div>

                  <div class="form-group">
                    <label>Last Name</label>
                    <input type="text" formControlName="lastName" placeholder="Doe">
                    <div class="error" *ngIf="passenger.get('lastName')?.hasError('required') && passenger.get('lastName')?.touched">
                      Required
                    </div>
                  </div>
                </div>

                <div class="form-group">
                  <label>Email</label>
                  <input type="email" formControlName="email" placeholder="john@example.com">
                  <div class="error" *ngIf="passenger.get('email')?.hasError('required') && passenger.get('email')?.touched">
                    Required
                  </div>
                </div>

                <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
                  <div class="form-group">
                    <label>Document Type</label>
                    <select formControlName="documentType">
                      <option value="NationalId" *ngIf="!isDomesticRoute()">National ID</option>
                      <option value="PassportNumber" *ngIf="isDomesticRoute()">Passport</option>
                      <option value="NationalId">National ID</option>
                      <option value="PassportNumber">Passport</option>
                    </select>
                  </div>

                  <div class="form-group">
                    <label>Document Number</label>
                    <input type="text" formControlName="documentNumber" 
                           [placeholder]="getDocumentPlaceholder(passenger.get('documentType')?.value)">
                    <div class="error" *ngIf="passenger.get('documentNumber')?.hasError('required') && passenger.get('documentNumber')?.touched">
                      Required
                    </div>
                    <div class="error" *ngIf="passenger.get('documentNumber')?.hasError('pattern') && passenger.get('documentNumber')?.touched">
                      Invalid format
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div style="display: flex; gap: 10px; margin-top: 30px;">
            <button type="submit" [disabled]="!form.valid || loading()" 
                    style="flex: 1; padding: 12px;">
              {{ loading() ? 'Processing...' : 'Confirm Booking' }}
            </button>
            <button type="button" class="btn-secondary" (click)="goBack()" style="padding: 12px;">
              Back
            </button>
          </div>
        </form>
      </div>
    </div>
  `
})
export class BookingComponent implements OnInit {
  form!: FormGroup;
  flight = signal<FlightResult | null>(null);
  originAirport = signal<Airport | null>(null);
  destAirport = signal<Airport | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

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
    const state = this.router.getCurrentNavigation()?.extras.state;
    if (!state?.flight) {
      this.router.navigate(['/flights']);
      return;
    }

    const flight = state.flight as FlightResult;
    this.flight.set(flight);
    this.initializeForm(flight);
  }

  private initializeForm(flight: FlightResult): void {
    const numPassengers = 1; // Get from search state if needed
    const passengersGroup = this.fb.array(
      Array.from({ length: numPassengers }, () =>
        this.createPassengerGroup()
      )
    );

    this.form = this.fb.group({
      passengers: passengersGroup
    });
  }

  private createPassengerGroup(): FormGroup {
    return this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      documentType: ['NationalId', Validators.required],
      documentNumber: ['', [Validators.required, Validators.pattern(/^[A-Za-z0-9]{6,12}$/)]]
    });
  }

  isDomesticRoute(): boolean {
    // Simplified - in real app, would check airport country codes
    return true;
  }

  getDocumentPlaceholder(docType?: string): string {
    if (docType === 'PassportNumber') return 'ABC123456';
    return 'ID12345678';
  }

  submitBooking(): void {
    if (!this.form.valid || !this.flight()) return;

    this.loading.set(true);
    this.error.set(null);

    const booking = {
      flightId: this.flight()!.flightId,
      departureDate: new Date().toISOString().split('T')[0], // Get from search state
      numberOfPassengers: this.passengersArray.length,
      passengers: this.passengersArray.value
    };

    this.bookingService.createBooking(booking).subscribe({
      next: (confirmation) => {
        this.router.navigate(['/confirmation'], { state: { confirmation } });
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Booking failed');
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/results']);
  }
}
