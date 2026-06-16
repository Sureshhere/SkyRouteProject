import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { FlightService } from '../services/flight.service';
import { Airport } from '../models';

function sameAirportValidator(control: AbstractControl): ValidationErrors | null {
  const origin = control.get('originAirportCode')?.value;
  const destination = control.get('destinationAirportCode')?.value;
  if (origin && destination && origin === destination) {
    return { sameAirport: true };
  }
  return null;
}

function pastDateValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value;
  if (!value) return null;
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  if (new Date(value) < today) {
    return { pastDate: true };
  }
  return null;
}

@Component({
  selector: 'app-flight-search',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="search-hero">
      <div class="container">
        <div class="hero-content">
          <h1 class="hero-title">✈ Find Your Perfect Flight</h1>
          <p class="hero-subtitle">Search and compare fares from GlobalAir and BudgetWings</p>
        </div>

        <div class="search-card">
          <div class="alert alert-error" *ngIf="error()">
            {{ error() }}
          </div>

          <form [formGroup]="form" (ngSubmit)="search()">
            <div class="search-form-row">
              <div class="form-group" style="margin-bottom: 0;">
                <label>From</label>
                <select formControlName="originAirportCode">
                  <option value="">Select departure airport</option>
                  <option *ngFor="let airport of airports()" [value]="airport.code">
                    {{ airport.code }} — {{ airport.city }}
                  </option>
                </select>
                <div class="error" *ngIf="form.get('originAirportCode')?.hasError('required') && form.get('originAirportCode')?.touched">
                  Please select your departure airport
                </div>
              </div>

              <div class="form-group" style="margin-bottom: 0;">
                <label>To</label>
                <select formControlName="destinationAirportCode">
                  <option value="">Select destination airport</option>
                  <option *ngFor="let airport of airports()" [value]="airport.code">
                    {{ airport.code }} — {{ airport.city }}
                  </option>
                </select>
                <div class="error" *ngIf="form.get('destinationAirportCode')?.hasError('required') && form.get('destinationAirportCode')?.touched">
                  Please select your destination airport
                </div>
              </div>

              <div class="form-group" style="margin-bottom: 0;">
                <label>Departure Date</label>
                <input type="date" formControlName="departureDate" [attr.min]="minDate">
                <div class="error" *ngIf="form.get('departureDate')?.hasError('required') && form.get('departureDate')?.touched">
                  Please select a departure date
                </div>
                <div class="error" *ngIf="form.get('departureDate')?.hasError('pastDate') && form.get('departureDate')?.touched">
                  Departure date cannot be in the past
                </div>
              </div>

              <div class="form-group" style="margin-bottom: 0;">
                <label>Passengers</label>
                <input type="number" formControlName="numberOfPassengers" min="1" max="9">
                <div class="error" *ngIf="form.get('numberOfPassengers')?.hasError('min') && form.get('numberOfPassengers')?.touched">
                  At least 1 passenger is required
                </div>
                <div class="error" *ngIf="form.get('numberOfPassengers')?.hasError('max') && form.get('numberOfPassengers')?.touched">
                  Maximum 9 passengers allowed
                </div>
              </div>

              <div class="form-group" style="margin-bottom: 0;">
                <label>Cabin Class</label>
                <select formControlName="cabinClass">
                  <option [value]="1">Economy</option>
                  <option [value]="2">Business</option>
                  <option [value]="3">First Class</option>
                </select>
              </div>

              <div class="form-group" style="margin-bottom: 0;">
                <label style="visibility: hidden;">Search</label>
                <button type="submit" class="btn-search" [disabled]="!form.valid || loading()">
                  {{ loading() ? 'Searching...' : 'Search Flights' }}
                </button>
              </div>
            </div>

            <div class="search-form-error" *ngIf="form.hasError('sameAirport') && form.get('destinationAirportCode')?.dirty">
              Origin and destination cannot be the same airport — please select two different airports
            </div>
          </form>
        </div>
      </div>
    </div>
  `
})
export class FlightSearchComponent implements OnInit {
  form: FormGroup;
  airports = signal<Airport[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  get minDate(): string {
    return new Date().toISOString().split('T')[0];
  }

  constructor(private fb: FormBuilder, private flightService: FlightService, private router: Router) {
    this.form = this.fb.group({
      originAirportCode: ['', Validators.required],
      destinationAirportCode: ['', Validators.required],
      departureDate: ['', [Validators.required, pastDateValidator]],
      numberOfPassengers: [1, [Validators.required, Validators.min(1), Validators.max(9)]],
      cabinClass: [1]
    }, { validators: sameAirportValidator });
  }

  ngOnInit(): void {
    this.loadAirports();
  }

  loadAirports(): void {
    this.flightService.getAirports().subscribe({
      next: (data) => this.airports.set(data),
      error: () => this.error.set('Unable to load airports. Please refresh the page and try again.')
    });
  }

  search(): void {
    if (!this.form.valid) return;

    this.loading.set(true);
    this.error.set(null);

    this.flightService.searchFlights(this.form.value).subscribe({
      next: (res) => {
        this.router.navigate(['/results'], {
          state: {
            flights: res.flights,
            search: this.form.value
          }
        });
      },
      error: (err: any) => {
        this.loading.set(false);
        this.error.set(this.extractError(err, 'Flight search failed. Please check your inputs and try again.'));
      }
    });
  }

  private extractError(err: any, fallback: string): string {
    if (Array.isArray(err.error?.errors) && err.error.errors.length > 0) {
      return err.error.errors.join(' ');
    }
    return err.error?.error || err.error?.message || fallback;
  }
}
