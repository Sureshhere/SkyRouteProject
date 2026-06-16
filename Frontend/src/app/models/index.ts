export interface AuthResponse {
  token: string;
  email: string;
  fullName: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface Airport {
  id: string;
  code: string;
  name: string;
  city: string;
  country: string;
  countryCode: string;
}

export interface FlightResult {
  flightId: string;
  airline: string;
  cabinClass: string;
  departureTime: string;
  arrivalTime: string;
  pricePerPassenger: number;
  totalPrice: number;
  pricingRule: string;
}

export interface FlightSearchRequest {
  originCode: string;
  destinationCode: string;
  departureDate: string;
  numberOfPassengers: number;
  cabinClass: string;
}

export interface PassengerDetail {
  firstName: string;
  lastName: string;
  email: string;
  documentType: string;
  documentNumber: string;
}

export interface CreateBookingRequest {
  flightId: string;
  departureDate: string;
  numberOfPassengers: number;
  passengers: PassengerDetail[];
}

export interface BookingConfirmation {
  id: string;
  bookingReferenceCode: string;
  flightId: string;
  airline: string;
  cabinClass: string;
  departureTime: string;
  arrivalTime: string;
  departureDate: string;
  numberOfPassengers: number;
  totalPrice: number;
  pricePerPassenger: number;
  status: string;
  createdAt: string;
}

export interface SortOption {
  label: string;
  value: 'price-asc' | 'price-desc' | 'duration-asc' | 'duration-desc' | 'time-asc';
}
