import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { BookingConfirmation, FlightService } from '../../services/flight.service';

@Component({
  selector: 'app-confirmation',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <div class="page-container">
        <div style="text-align: center; margin-bottom: 40px;">
          <div style="font-size: 60px; color: #16a34a; margin-bottom: 20px;">✓</div>
          <h2>Booking Confirmed!</h2>
        </div>

        <div class="card" *ngIf="confirmation()" style="max-width: 600px; margin: 0 auto;">
          <div style="background: #f0f9ff; padding: 20px; border-radius: 8px; margin-bottom: 20px; border-left: 4px solid #0284c7;">
            <div style="font-size: 12px; color: #0c4a6e; margin-bottom: 8px;">BOOKING REFERENCE</div>
            <div style="font-size: 24px; font-weight: bold; color: #0c4a6e; font-family: monospace;">
              {{ confirmation()!.bookingReferenceCode }}
            </div>
          </div>

          <div style="margin-bottom: 20px;">
            <h3 style="margin-top: 0; margin-bottom: 12px;">Flight Details</h3>
            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
              <div>
                <small style="color: #6b7280;">Airline</small><br>
                <strong>{{ confirmation()!.airline }}</strong>
              </div>
              <div>
                <small style="color: #6b7280;">Cabin Class</small><br>
                <strong>{{ confirmation()!.cabinClass }}</strong>
              </div>
              <div>
                <small style="color: #6b7280;">Departure</small><br>
                <strong>{{ confirmation()!.departureTime }}</strong>
              </div>
              <div>
                <small style="color: #6b7280;">Arrival</small><br>
                <strong>{{ confirmation()!.arrivalTime }}</strong>
              </div>
            </div>
          </div>

          <div style="margin-bottom: 20px; padding: 20px; background: #f9fafb; border-radius: 8px;">
            <div style="display: flex; justify-content: space-between; margin-bottom: 8px;">
              <span>Passengers</span>
              <strong>{{ confirmation()!.numberOfPassengers }}</strong>
            </div>
            <div style="display: flex; justify-content: space-between; margin-bottom: 8px;">
              <span>Price per Passenger</span>
              <strong>{{ formatPrice(confirmation()!.pricePerPassenger) }}</strong>
            </div>
            <div style="border-top: 1px solid #e5e7eb; padding-top: 8px; display: flex; justify-content: space-between;">
              <span style="font-weight: 600;">Total Price</span>
              <strong style="font-size: 18px; color: #1e3a8a;">{{ formatPrice(confirmation()!.totalPrice) }}</strong>
            </div>
          </div>

          <div style="text-align: center; padding: 20px; background: #f3f4f6; border-radius: 8px; margin-bottom: 20px;">
            <small style="color: #6b7280;">Status</small><br>
            <strong style="color: #16a34a;">{{ confirmation()!.status }}</strong>
          </div>

          <p style="text-align: center; color: #6b7280; font-size: 14px; margin: 20px 0;">
            A confirmation email has been sent to your registered email address. Please save your booking reference code.
          </p>

          <div style="display: flex; gap: 10px;">
            <button (click)="goHome()" style="flex: 1;">Back to Home</button>
            <button (click)="searchMore()" class="btn-secondary" style="flex: 1;">Search More Flights</button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ConfirmationComponent implements OnInit {
  confirmation = signal<any>(null);

  constructor(private router: Router) {}

  ngOnInit(): void {
    const state = this.router.getCurrentNavigation()?.extras.state;
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
