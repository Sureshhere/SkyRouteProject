import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateBookingRequest, BookingConfirmation } from '../models';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private apiUrl = 'http://localhost:5235/api/bookings';

  constructor(private http: HttpClient, private authService: AuthService) { }

  createBooking(req: CreateBookingRequest): Observable<BookingConfirmation> {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
    return this.http.post<BookingConfirmation>(this.apiUrl, req, { headers });
  }

  getBooking(id: string): Observable<BookingConfirmation> {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
    return this.http.get<BookingConfirmation>(`${this.apiUrl}/${id}`, { headers });
  }

  cancelBooking(id: string): Observable<{ message: string }> {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
    return this.http.delete<{ message: string }>(`${this.apiUrl}/${id}`, { headers });
  }

  isDomesticRoute(originCountryCode: string, destCountryCode: string): boolean {
    return originCountryCode.toUpperCase() === destCountryCode.toUpperCase();
  }

  validateDocumentNumber(documentType: string, documentNumber: string): boolean {
    if (documentType === 'NationalId') {
      return /^[A-Za-z0-9]{8,12}$/.test(documentNumber);
    } else if (documentType === 'PassportNumber') {
      return /^[A-Za-z0-9]{6,9}$/.test(documentNumber);
    }
    return false;
  }
}
