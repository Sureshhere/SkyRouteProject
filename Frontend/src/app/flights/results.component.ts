import { Component, signal, effect, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FlightService } from '../../services/flight.service';
import { FlightResult, SortOption } from '../../models';

@Component({
  selector: 'app-flight-results',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <div class="page-container">
        <h2>Search Results</h2>

        <div class="alert alert-info">
          Found {{ displayedFlights().length }} flights
        </div>

        <div class="sort-controls">
          <button *ngFor="let opt of sortOptions" 
                  (click)="setSortBy(opt.value)"
                  [class.active]="sortBy() === opt.value"
                  class="sort-btn">
            {{ opt.label }}
          </button>
        </div>

        <div *ngIf="!displayedFlights().length" class="empty-state">
          <h3>No flights found</h3>
          <p>Try adjusting your search criteria</p>
          <button class="btn-secondary" (click)="goBack()">Back to Search</button>
        </div>

        <div class="grid" *ngIf="displayedFlights().length">
          <div *ngFor="let flight of displayedFlights()" 
               class="flight-card"
               (click)="selectFlight(flight)">
            <div class="flight-header">
              <div>
                <div class="airline">{{ flight.airline }}</div>
                <div class="cabin-class">{{ flight.cabinClass }}</div>
              </div>
              <div style="text-align: right;">
                <div class="price">{{ flightService.formatPrice(flight.totalPrice) }}</div>
                <div class="price-per-person">{{ flightService.formatPrice(flight.pricePerPassenger) }} per person</div>
              </div>
            </div>

            <div class="flight-times">
              <div>
                <div class="time">{{ flightService.formatTime(flight.departureTime) }}</div>
                <div style="font-size: 12px; color: #6b7280;">Depart</div>
              </div>
              <div class="duration">
                {{ getDuration(flight.departureTime, flight.arrivalTime) }}
              </div>
              <div>
                <div class="time">{{ flightService.formatTime(flight.arrivalTime) }}</div>
                <div style="font-size: 12px; color: #6b7280;">Arrive</div>
              </div>
            </div>

            <div style="margin-top: 12px; padding-top: 12px; border-top: 1px solid #f0f0f0;">
              <small style="color: #6b7280;">{{ flight.pricingRule }}</small>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class FlightResultsComponent implements OnInit {
  flights = signal<FlightResult[]>([]);
  sortBy = signal<string>('price-asc');
  displayedFlights = signal<FlightResult[]>([]);
  sortOptions: SortOption[] = [
    { label: 'Price: Low to High', value: 'price-asc' },
    { label: 'Price: High to Low', value: 'price-desc' },
    { label: 'Duration: Shortest', value: 'duration-asc' },
    { label: 'Duration: Longest', value: 'duration-desc' },
    { label: 'Departure: Earliest', value: 'time-asc' }
  ];

  constructor(public flightService: FlightService, private router: Router) {
    effect(() => {
      const sorted = this.flightService.sortFlights(this.flights(), this.sortBy());
      this.displayedFlights.set(sorted);
    });
  }

  ngOnInit(): void {
    const state = this.router.getCurrentNavigation()?.extras.state;
    if (state?.flights) {
      this.flights.set(state.flights);
    } else {
      this.router.navigate(['/flights']);
    }
  }

  setSortBy(value: string): void {
    this.sortBy.set(value);
  }

  getDuration(departure: string, arrival: string): string {
    const dep = new Date(departure);
    const arr = new Date(arrival);
    const ms = arr.getTime() - dep.getTime();
    const hours = Math.floor(ms / (1000 * 60 * 60));
    const minutes = Math.floor((ms % (1000 * 60 * 60)) / (1000 * 60));
    return `${hours}h ${minutes}m`;
  }

  selectFlight(flight: FlightResult): void {
    this.router.navigate(['/booking'], { state: { flight } });
  }

  goBack(): void {
    this.router.navigate(['/flights']);
  }
}
