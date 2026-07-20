import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { BookingConfirmation } from '../models';

@Component({
  selector: 'app-confirmation',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="confirmation-page">

      <div style="text-align: center; margin-bottom: 28px;">
        <div class="confirmation-icon">\u2713</div>
        <h2 style="font-size: 26px; font-weight: 800; color: var(--text);">Booking Confirmed!</h2>
        <p style="color: var(--text-muted); margin-top: 6px;">Your flight has been successfully booked</p>
      </div>

      <div class="confirmation-card" *ngIf="confirmation()">

        <div class="booking-ref-banner">
          <div class="booking-ref-label">Booking Reference</div>
          <div class="booking-ref-code">{{ confirmation()!.bookingReferenceCode }}</div>
        </div>

        <div class="confirmation-body">

          <div class="info-section-title">Flight Details</div>
          <div class="info-row">
            <span class="info-label">Airline</span>
            <span class="info-value">{{ confirmation()!.flightDetails.airlineName }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">Flight</span>
            <span class="info-value">{{ confirmation()!.flightDetails.flightNumber }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">Route</span>
            <span class="info-value">{{ confirmation()!.flightDetails.origin }} \u2192 {{ confirmation()!.flightDetails.destination }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">Cabin Class</span>
            <span class="info-value">{{ confirmation()!.flightDetails.cabinClass }}</span>
          </div>

          <div class="info-section-title">Pricing</div>
          <div class="info-row">
            <span class="info-label">Passengers</span>
            <span class="info-value">{{ confirmation()!.pricing.numberOfPassengers }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">Price per Passenger</span>
            <span class="info-value">{{ formatPrice(confirmation()!.pricing.pricePerPassenger) }}</span>
          </div>
          <div class="price-total-row">
            <span>Total</span>
            <span class="price-total-amount">{{ formatPrice(confirmation()!.pricing.totalPrice) }}</span>
          </div>

          <div *ngIf="confirmation()!.passengers && confirmation()!.passengers!.length > 0" style="margin-top: 20px;">
            <div style="font-weight: 600; margin-bottom: 8px;">Passengers</div>
            <div *ngFor="let p of confirmation()!.passengers; let i = index" style="padding: 6px 0; border-bottom: 1px solid #eee;">
              Passenger {{ i + 1 }} — {{ p.fullName }} · Seat: {{ p.seatNumber || '—' }}
            </div>
          </div>

          <div style="text-align: center; margin: 20px 0;">
            <span class="status-badge">\u2713 {{ confirmation()!.bookingStatus }}</span>
          </div>

          <p style="text-align: center; color: var(--text-muted); font-size: 13px; margin-bottom: 20px;">
            Save your booking reference code above. A confirmation has been sent to your email.
          </p>

          <div style="display: flex; gap: 12px;">
            <button (click)="goHome()" style="flex: 1; background: var(--primary); padding: 12px;">Back to Home</button>
            <button (click)="searchMore()" class="btn-secondary" style="flex: 1; padding: 12px;">Search More</button>
          </div>

        </div>
      </div>
    </div>
  `
})
export class ConfirmationComponent implements OnInit {
  confirmation = signal<BookingConfirmation | null>(null);

  constructor(private router: Router) {}

  ngOnInit(): void {
    const state = history.state;
    if (!state?.confirmation) {
      this.router.navigate(['/flights']);
      return;
    }
    this.confirmation.set(state.confirmation);
  }

  formatPrice(price: number): string {
    return `USD ${price.toFixed(2)}`;
  }

  goHome(): void {
    this.router.navigate(['/flights']);
  }

  searchMore(): void {
    this.router.navigate(['/flights']);
  }
}
