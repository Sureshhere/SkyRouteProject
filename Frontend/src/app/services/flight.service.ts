import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Airport, FlightResult, FlightSearchRequest } from '../models';

@Injectable({
  providedIn: 'root'
})
export class FlightService {
  private apiUrl = 'http://localhost:5235/api';

  constructor(private http: HttpClient) { }

  getAirports(): Observable<Airport[]> {
    return this.http.get<Airport[]>(`${this.apiUrl}/airports`);
  }

  searchFlights(req: FlightSearchRequest): Observable<{ flights: FlightResult[] }> {
    return this.http.post<{ flights: FlightResult[] }>(`${this.apiUrl}/flights/search`, req);
  }

  sortFlights(flights: FlightResult[], sortBy: string): FlightResult[] {
    const sorted = [...flights];

    switch (sortBy) {
      case 'price-asc':
        return sorted.sort((a, b) => a.pricing.totalPrice - b.pricing.totalPrice);
      case 'price-desc':
        return sorted.sort((a, b) => b.pricing.totalPrice - a.pricing.totalPrice);
      case 'duration-asc':
        return sorted.sort((a, b) => a.durationMinutes - b.durationMinutes);
      case 'duration-desc':
        return sorted.sort((a, b) => b.durationMinutes - a.durationMinutes);
      case 'time-asc':
        return sorted.sort((a, b) => a.departureTime.localeCompare(b.departureTime));
      default:
        return sorted;
    }
  }

  formatPrice(price: number): string {
    return `USD ${price.toFixed(2)}`;
  }

  formatTime(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  }
}
