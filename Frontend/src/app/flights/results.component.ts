import { Component, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FlightService } from '../services/flight.service';
import { FlightResult, SortOption } from '../models';

@Component({
  selector: 'app-flight-results',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <div class="page-container">

        <div class="results-meta">
          <span class="results-count">
            <strong>{{ displayedFlights().length }}</strong> flights found
          </span>
          <button class="btn-secondary" (click)="goBack()" style="font-size: 13px; padding: 7px 16px;">
            ← Modify Search
          </button>
        </div>

        <div class="sort-controls">
          <button *ngFor="let opt of sortOptions"
                  (click)="setSortBy(opt.value)"
                  [class.active]="sortBy() === opt.value"
                  class="sort-chip">
            {{ opt.label }}
          </button>
        </div>

        <div *ngIf="!displayedFlights().length" class="empty-state">
          <h3>No flights found</h3>
          <p>Try adjusting your search criteria</p>
          <button class="btn-secondary" (click)="goBack()" style="margin-top: 16px;">Back to Search</button>
        </div>

        <div class="flight-list" *ngIf="displayedFlights().length">
          <div *ngFor="let flight of displayedFlights()" class="flight-row" (click)="selectFlight(flight)">

            <div class="flight-airline">
              <div class="airline-name">{{ flight.airlineName }}</div>
              <div class="flight-number-badge">{{ flight.flightNumber }}</div>
              <span class="cabin-badge">{{ flight.cabinClass }}</span>
            </div>

            <div class="flight-route">
              <div class="route-point">
                <div class="route-time">{{ flightService.formatTime(flight.departureTime) }}</div>
                <div class="route-code">{{ flight.originCode }}</div>
              </div>

              <div class="route-timeline">
                <div class="timeline-duration">{{ getDuration(flight.durationMinutes) }}</div>
                <div class="timeline-bar">
                  <div class="timeline-dot"></div>
                  <div class="timeline-line"></div>
                  <span class="timeline-plane">✈</span>
                  <div class="timeline-line"></div>
                  <div class="timeline-dot"></div>
                </div>
              </div>

              <div class="route-point">
                <div class="route-time">{{ flightService.formatTime(flight.arrivalTime) }}</div>
                <div class="route-code">{{ flight.destinationCode }}</div>
              </div>
            </div>

            <div class="flight-pricing">
              <div class="price-total">{{ formatPrice(flight.pricing.totalPrice) }}</div>
              <div class="price-per-pax">{{ formatPrice(flight.pricing.pricePerPassenger) }} / person</div>
              <div class="pricing-rule-tag">{{ flight.pricing.pricingRule }}</div>
              <button class="btn-select" (click)="$event.stopPropagation(); selectFlight(flight)">Select →</button>
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
  displayedFlights = computed(() => this.flightService.sortFlights(this.flights(), this.sortBy()));
  searchParams: any = null;
  sortOptions: SortOption[] = [
    { label: 'Price: Low to High', value: 'price-asc' },
    { label: 'Price: High to Low', value: 'price-desc' },
    { label: 'Duration: Shortest', value: 'duration-asc' },
    { label: 'Duration: Longest', value: 'duration-desc' },
    { label: 'Departure: Earliest', value: 'time-asc' }
  ];

  constructor(public flightService: FlightService, private router: Router) {}

  ngOnInit(): void {
    const state = history.state;
    if (state?.flights) {
      this.flights.set(state.flights);
      this.searchParams = state.search;
    } else {
      this.router.navigate(['/flights']);
    }
  }

  setSortBy(value: string): void {
    this.sortBy.set(value);
  }

  getDuration(minutes: number): string {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return `${hours}h ${mins}m`;
  }

  formatPrice(price: number): string {
    return `USD ${price.toFixed(2)}`;
  }

  selectFlight(flight: FlightResult): void {
    this.router.navigate(['/booking'], { state: { flight, search: this.searchParams } });
  }

  goBack(): void {
    this.router.navigate(['/flights']);
  }
}
