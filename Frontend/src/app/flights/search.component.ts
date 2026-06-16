import { Component, signal, effect, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { FlightService } from '../../services/flight.service';
import { Airport } from '../../models';

@Component({
  selector: 'app-flight-search',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="container">
      <div class="page-container">
        <h2>Search Flights</h2>

        <div class="alert alert-error" *ngIf="error()">
          {{ error() }}
        </div>

        <div class="card">
          <form [formGroup]="form" (ngSubmit)="search()">
            <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px;">
              <div class="form-group">
                <label>From</label>
                <select formControlName="originCode">
                  <option value="">Select origin...</option>
                  <option *ngFor="let airport of airports()" [value]="airport.code">
                    {{ airport.code }} - {{ airport.city }}
                  </option>
                </select>
                <div class="error" *ngIf="form.get('originCode')?.hasError('required') && form.get('originCode')?.touched">
                  Origin is required
                </div>
              </div>

              <div class="form-group">
                <label>To</label>
                <select formControlName="destinationCode">
                  <option value="">Select destination...</option>
                  <option *ngFor="let airport of airports()" [value]="airport.code">
                    {{ airport.code }} - {{ airport.city }}
                  </option>
                </select>
                <div class="error" *ngIf="form.get('destinationCode')?.hasError('required') && form.get('destinationCode')?.touched">
                  Destination is required
                </div>
              </div>

              <div class="form-group">
                <label>Departure Date</label>
                <input type="date" formControlName="departureDate">
                <div class="error" *ngIf="form.get('departureDate')?.hasError('required') && form.get('departureDate')?.touched">
                  Date is required
                </div>
              </div>

              <div class="form-group">
                <label>Passengers</label>
                <input type="number" formControlName="numberOfPassengers" min="1" max="9">
                <div class="error" *ngIf="form.get('numberOfPassengers')?.hasError('required') && form.get('numberOfPassengers')?.touched">
                  Passengers required
                </div>
              </div>

              <div class="form-group">
                <label>Cabin Class</label>
                <select formControlName="cabinClass">
                  <option value="Economy">Economy</option>
                  <option value="Business">Business</option>
                  <option value="FirstClass">First Class</option>
                </select>
              </div>

              <div style="display: flex; align-items: flex-end;">
                <button type="submit" [disabled]="!form.valid || loading()" style="width: 100%;">
                  {{ loading() ? 'Searching...' : 'Search Flights' }}
                </button>
              </div>
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

  constructor(private fb: FormBuilder, private flightService: FlightService, private router: Router) {
    this.form = this.fb.group({
      originCode: ['', Validators.required],
      destinationCode: ['', Validators.required],
      departureDate: ['', Validators.required],
      numberOfPassengers: [1, [Validators.required, Validators.min(1), Validators.max(9)]],
      cabinClass: ['Economy']
    });
  }

  ngOnInit(): void {
    this.loadAirports();
  }

  loadAirports(): void {
    this.flightService.getAirports().subscribe({
      next: (data) => this.airports.set(data),
      error: (err) => this.error.set('Failed to load airports')
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
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Search failed. Try again.');
      }
    });
  }
}
