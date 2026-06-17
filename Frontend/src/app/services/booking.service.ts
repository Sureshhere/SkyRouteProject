import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateBookingRequest, BookingConfirmation } from '../models';

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private apiUrl = 'http://localhost:5235/api/bookings';

  constructor(private http: HttpClient) { }

  createBooking(req: CreateBookingRequest): Observable<BookingConfirmation> {
    return this.http.post<BookingConfirmation>(this.apiUrl, req, { withCredentials: true });
  }

  getBooking(id: string): Observable<BookingConfirmation> {
    return this.http.get<BookingConfirmation>(`${this.apiUrl}/${id}`, { withCredentials: true });
  }

  cancelBooking(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/${id}`, { withCredentials: true });
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
