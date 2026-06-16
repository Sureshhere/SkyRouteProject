import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Airport, FlightResult, FlightSearchRequest } from '../models';

@Injectable({
  providedIn: 'root'
})
export class FlightService {
  private apiUrl = 'http://localhost:5000/api';

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
        return sorted.sort((a, b) => a.totalPrice - b.totalPrice);
      case 'price-desc':
        return sorted.sort((a, b) => b.totalPrice - a.totalPrice);
      case 'duration-asc':
        return sorted.sort((a, b) => {
          const durationA = this.getDuration(a.departureTime, a.arrivalTime);
          const durationB = this.getDuration(b.departureTime, b.arrivalTime);
          return durationA - durationB;
        });
      case 'duration-desc':
        return sorted.sort((a, b) => {
          const durationA = this.getDuration(a.departureTime, a.arrivalTime);
          const durationB = this.getDuration(b.departureTime, b.arrivalTime);
          return durationB - durationA;
        });
      case 'time-asc':
        return sorted.sort((a, b) => a.departureTime.localeCompare(b.departureTime));
      default:
        return sorted;
    }
  }

  private getDuration(departure: string, arrival: string): number {
    const depTime = new Date(departure);
    const arrTime = new Date(arrival);
    return arrTime.getTime() - depTime.getTime();
  }

  formatPrice(price: number): string {
    return `USD ${price.toFixed(2)}`;
  }

  formatTime(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  }
}
