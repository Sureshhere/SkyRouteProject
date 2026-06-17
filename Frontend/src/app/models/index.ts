export interface AuthResponse {
  id: string;
  expiresIn: number;
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
  id: string;
  airlineName: string;
  airlineCode: string;
  flightNumber: string;
  originCode: string;
  destinationCode: string;
  departureTime: string;
  arrivalTime: string;
  durationMinutes: number;
  cabinClass: string;
  pricing: {
    baseFare: number;
    pricePerPassenger: number;
    totalPrice: number;
    pricingRule: string;
  };
}

export interface FlightSearchRequest {
  originAirportCode: string;
  destinationAirportCode: string;
  departureDate: string;
  numberOfPassengers: number;
  cabinClass: number;
}

export interface PassengerDetail {
  fullName: string;
  email: string;
  documentNumber: string;
}

export interface CreateBookingRequest {
  flightId: string;
  departureDate: string;
  passengers: PassengerDetail[];
}

export interface BookingConfirmation {
  bookingId: string;
  bookingReferenceCode: string;
  flightDetails: {
    airlineName: string;
    flightNumber: string;
    origin: string;
    destination: string;
    departureTime: string;
    arrivalTime: string;
    cabinClass: string;
  };
  pricing: {
    totalPrice: number;
    pricePerPassenger: number;
    numberOfPassengers: number;
  };
  bookingStatus: string;
  createdAt: string;
}

export interface SortOption {
  label: string;
  value: 'price-asc' | 'price-desc' | 'duration-asc' | 'duration-desc' | 'time-asc';
}
