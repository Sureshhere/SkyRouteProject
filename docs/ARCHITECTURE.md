# SkyRoute Backend Architecture Design

## 1. System Architecture Overview

### High-Level Architecture
```
┌─────────────────────────────────────────────────────────────┐
│                    Angular Frontend (Phase 2)               │
└─────────────────────────────────────────────────────────────┘
                              ↓
                        (HTTP/HTTPS)
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                  ASP.NET Core Web API                       │
├─────────────────────────────────────────────────────────────┤
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  Controllers │→ │   Services   │→ │   Data Ctx   │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
├─────────────────────────────────────────────────────────────┤
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  DTOs/Models │  │ Validators   │  │ Auth/JWT     │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
├─────────────────────────────────────────────────────────────┤
│         Provider Abstraction Layer                           │
│  ┌──────────────┐  ┌──────────────┐                         │
│  │  GlobalAir   │  │ BudgetWings  │                         │
│  └──────────────┘  └──────────────┘                         │
└─────────────────────────────────────────────────────────────┘
                              ↓
                        SQL Server
                  (Airports, Users, Flights, Bookings)
```

### Request Flow - Flight Search (Public)
```
GET /api/flights/search?origin=NYC&destination=LAX&date=2026-07-15&passengers=2&cabin=Economy
                    ↓
            [No Auth Required]
                    ↓
         FlightController.Search()
                    ↓
         FlightSearchService.SearchFlights()
                    ↓
    ProviderFactory.GetProviders() → [GlobalAir, BudgetWings]
                    ↓
   Each provider calculates pricing independently
                    ↓
   Combine results + map to SearchResultDTO
                    ↓
   Return [total_price, per_passenger_price, flights...]
```

### Request Flow - Booking (Authenticated)
```
POST /api/bookings with JWT token
                    ↓
         [JWT Validation Middleware]
                    ↓
    BookingController.CreateBooking()
                    ↓
      BookingService.ValidateAndBook()
                    ↓
   - Validate flight availability
   - Validate passenger details
   - Validate document type (domestic vs international)
                    ↓
       Save to database + return reference code
```

---

## 2. Database Schema Design

### Core Tables

#### Users Table
```sql
Users
├── Id (GUID, PK)
├── Email (string, unique, indexed)
├── PasswordHash (string)
├── PasswordSalt (string)
├── FullName (string)
├── CreatedAt (DateTime)
└── IsActive (bool)
```

#### Airports Table
```sql
Airports
├── Id (GUID, PK)
├── Code (string, unique, e.g., "NYC")
├── Name (string, e.g., "New York LaGuardia")
├── CountryCode (string, e.g., "US")
├── Country (string, e.g., "United States")
├── City (string)
└── IsActive (bool)
```

#### Airlines Table
```sql
Airlines
├── Id (GUID, PK)
├── Name (string, unique - "GlobalAir", "BudgetWings")
├── Code (string, unique)
└── IsActive (bool)
```

#### Flights Table
```sql
Flights
├── Id (GUID, PK)
├── AirlineId (GUID, FK → Airlines)
├── FlightNumber (string)
├── OriginAirportId (GUID, FK → Airports)
├── DestinationAirportId (GUID, FK → Airports)
├── DepartureTime (DateTime)
├── ArrivalTime (DateTime)
├── Duration (TimeSpan)
├── CabinClass (enum: Economy, Business, FirstClass)
├── BaseFare (decimal)
├── Capacity (int)
├── IsActive (bool)
└── CreatedAt (DateTime)
```

#### Bookings Table
```sql
Bookings
├── Id (GUID, PK)
├── BookingReferenceCode (string, unique - auto-generated)
├── UserId (GUID, FK → Users)
├── FlightId (GUID, FK → Flights)
├── NumberOfPassengers (int)
├── TotalPrice (decimal)
├── PricePerPassenger (decimal)
├── Status (enum: Confirmed, Cancelled)
├── CreatedAt (DateTime)
└── UpdatedAt (DateTime)
```

#### PassengerDetails Table
```sql
PassengerDetails
├── Id (GUID, PK)
├── BookingId (GUID, FK → Bookings)
├── FullName (string)
├── Email (string)
├── DocumentType (enum: NationalID, PassportNumber)
├── DocumentNumber (string)
├── SeatNumber (NVARCHAR(5), NOT NULL, default '')
├── PassengerIndex (int)
└── CreatedAt (DateTime)
```

### Rationale
- **GUID** for primary keys: Allows data generation without central sequence; better for distributed systems
- **CountryCode** in Airports: Essential for domestic vs international detection
- **DocumentType enum**: Simplifies validation logic; determines required format
- **Booking Reference Code**: Unique identifier for user-facing reference
- **Passenger Details separated**: Supports multiple passengers per booking

---

## 3. API Contract Design

### Authentication Endpoints

#### POST /api/auth/register
**Public Endpoint**
```json
Request:
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "fullName": "John Doe"
}

Response (201):
{
  "id": "user-id-guid",
  "email": "user@example.com",
  "fullName": "John Doe",
  "token": "eyJhbGciOiJIUzI1NiIs..."
}

Error Response (400/409):
{
  "error": "Email already registered"
}
```

#### POST /api/auth/login
**Public Endpoint**
```json
Request:
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}

Response (200):
{
  "id": "user-id-guid",
  "email": "user@example.com",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600
}

Error Response (401):
{
  "error": "Invalid credentials"
}
```

### Flight Search Endpoints

#### GET /api/airports
**Public Endpoint**
```json
Response (200):
[
  {
    "id": "airport-id-guid",
    "code": "NYC",
    "name": "New York LaGuardia",
    "city": "New York",
    "country": "United States",
    "countryCode": "US"
  },
  { ... }
]
```

#### POST /api/flights/search
**Public Endpoint**
```json
Request:
{
  "originAirportCode": "NYC",
  "destinationAirportCode": "LAX",
  "departureDate": "2026-07-15",
  "numberOfPassengers": 2,
  "cabinClass": "Economy"
}

Response (200):
{
  "flights": [
    {
      "id": "flight-id-guid",
      "airlineName": "GlobalAir",
      "airlineCode": "GA",
      "flightNumber": "GA-101",
      "departureTime": "2026-07-15T08:00:00Z",
      "arrivalTime": "2026-07-15T11:30:00Z",
      "durationMinutes": 210,
      "cabinClass": "Economy",
      "pricing": {
        "baseFare": 150.00,
        "totalPrice": 345.00,
        "pricePerPassenger": 172.50,
        "breakdown": {
          "provider": "GlobalAir",
          "rule": "Base fare + 15% fuel surcharge",
          "calculation": "150 * 1.15 * 2 passengers = 345.00"
        }
      }
    },
    {
      "id": "flight-id-guid-2",
      "airlineName": "BudgetWings",
      "airlineCode": "BW",
      "flightNumber": "BW-202",
      "departureTime": "2026-07-15T09:00:00Z",
      "arrivalTime": "2026-07-15T12:15:00Z",
      "durationMinutes": 195,
      "cabinClass": "Economy",
      "pricing": {
        "baseFare": 150.00,
        "totalPrice": 270.00,
        "pricePerPassenger": 135.00,
        "breakdown": {
          "provider": "BudgetWings",
          "rule": "Base fare - 10% discount (min $29.99)",
          "calculation": "(150 - 15) * 2 passengers = 270.00"
        }
      }
    }
  ],
  "searchMetadata": {
    "origin": "NYC",
    "destination": "LAX",
    "departureDate": "2026-07-15",
    "passengers": 2,
    "cabin": "Economy",
    "resultsCount": 2
  }
}

Error Response (400):
{
  "error": "Invalid search parameters",
  "details": ["Departure date must be in the future"]
}
```

#### GET /api/flights/{flightId}/seats?departureDate
**Authenticated Endpoint (Requires JWT Token)**
```json
Response (200):
{
  "flightId": "flight-id-guid",
  "departureDate": "2026-07-15",
  "cabinClass": "Economy",
  "availableSeats": ["1A", "1B", "2A", "2B", "3C"]
}

Error Response (404):
{
  "error": "Flight not found"
}
```

### Booking Endpoints

#### POST /api/bookings
**Authenticated Endpoint (Requires JWT Token)**
```json
Request (with Authorization: Bearer {token}):
{
  "flightId": "flight-id-guid",
  "passengers": [
    {
      "fullName": "John Doe",
      "email": "john@example.com",
      "documentType": "NationalID",
      "documentNumber": "AB123456",
      "seatNumber": "12A"
    },
    {
      "fullName": "Jane Doe",
      "email": "jane@example.com",
      "documentType": "NationalID",
      "documentNumber": "CD789012",
      "seatNumber": "12B"
    }
  ]
}

Response (201):
{
  "bookingId": "booking-id-guid",
  "bookingReferenceCode": "SK20260715001",
  "flightDetails": {
    "airlineName": "GlobalAir",
    "flightNumber": "GA-101",
    "origin": "NYC",
    "destination": "LAX",
    "departureTime": "2026-07-15T08:00:00Z",
    "arrivalTime": "2026-07-15T11:30:00Z"
  },
  "pricing": {
    "totalPrice": 345.00,
    "pricePerPassenger": 172.50,
    "numberOfPassengers": 2
  },
  "passengers": [
    {
      "fullName": "John Doe",
      "email": "john@example.com",
      "documentType": "NationalID",
      "seatNumber": "12A"
    },
    {
      "fullName": "Jane Doe",
      "email": "jane@example.com",
      "documentType": "NationalID",
      "seatNumber": "12B"
    }
  ],
  "bookingStatus": "Confirmed",
  "createdAt": "2026-06-16T10:30:00Z"
}

Error Response (400):
{
  "error": "Validation failed",
  "details": ["Document number format invalid for domestic flight"]
}

Error Response (401):
{
  "error": "Unauthorized - Please login to book flights"
}

Error Response (404):
{
  "error": "Flight not found"
}

Error Response (409):
{
  "error": "One or more selected seats are no longer available"
}
```

#### GET /api/bookings/{bookingReferenceCode}
**Authenticated Endpoint (Owner-Only Access)**
```json
Response (200): [Same as booking response above]

Error Response (401/403):
{
  "error": "Unauthorized - You can only view your own bookings"
}
```

---

## 4. Design Patterns Implementation

### Pattern 1: Strategy Pattern (Provider Pricing)
**Purpose:** Isolate provider-specific pricing logic; easy to add new providers

```
IFlightPricingStrategy
├── GlobalAirPricingStrategy (Base + 15% surcharge)
├── BudgetWingsPricingStrategy (Base - 10% discount, min $29.99)
└── [Future]NewProviderPricingStrategy

FlightService uses:
  - IEnumerable<IFlightPricingStrategy> strategies
  - ApplyPricing(baseFare, passengers) → decimal
```

**Benefit:** New provider = new strategy class only, no existing code modification (Open/Closed Principle)

### Pattern 2: Factory Pattern (Provider Creation)
**Purpose:** Centralize provider instantiation; maintain single source of truth

```
IFlightProviderFactory
  └── CreateProviders() → IEnumerable<IFlightProvider>

FlightProviderFactory
  ├── Register("GlobalAir", new GlobalAirProvider())
  ├── Register("BudgetWings", new BudgetWingsProvider())
  └── GetAll() → List<IFlightProvider>
```

**Benefit:** Easy registration/removal of providers; dependency injection friendly

### Pattern 3: Repository Pattern (Data Access)
**Purpose:** Abstract data access; centralize query logic; easier testing

```
IFlightRepository
  ├── GetByIdAsync(id)
  ├── GetByRouteAndDateAsync(origin, dest, date)
  ├── AddAsync(flight)
  └── SaveAsync()

IBookingRepository
  ├── GetByIdAsync(id)
  ├── GetByUserIdAsync(userId)
  ├── AddAsync(booking)
  └── SaveAsync()
```

**Benefit:** Queries centralized; easier to modify queries across app; testable with mock repos

---

## 5. Authentication & JWT Design

### JWT Token Structure
```json
Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload:
{
  "sub": "user-id-guid",
  "email": "user@example.com",
  "fullName": "John Doe",
  "iat": 1718537400,
  "exp": 1718541000,
  "iss": "SkyRoute",
  "aud": "SkyRoute"
}

Signature: HMACSHA256(base64url(header) + "." + base64url(payload), secret)
```

### Token Claims
- **sub** (subject): User ID
- **email**: User email
- **fullName**: User full name
- **iat** (issued at): Token creation timestamp
- **exp** (expiration): 1 hour from creation
- **iss** (issuer): "SkyRoute"
- **aud** (audience): "SkyRoute"

### Authorization Flow
```
1. User logs in → POST /api/auth/login
2. Backend validates credentials
3. Generate JWT token (expires in 1 hour)
4. Return token to frontend
5. Frontend stores token (localStorage/sessionStorage)
6. Frontend includes token in Authorization header: "Bearer {token}"
7. Middleware validates token on protected endpoints
8. If valid, extract UserId from claims → inject into controller
9. If expired/invalid, return 401 Unauthorized
```

### Endpoint Authorization
```
[AllowAnonymous]
- POST /api/auth/register
- POST /api/auth/login
- GET /api/airports
- POST /api/flights/search

[Authorize]
- POST /api/bookings
- GET /api/bookings/{code}
```

---

## 6. Pricing Calculation Architecture

### Calculation Flow
```
BasePrice = Flight.BaseFare
PassengerCount = Search.NumberOfPassengers

GlobalAir Calculation:
  ├── FuelSurcharge = BasePrice * 0.15
  ├── SurchargedPrice = BasePrice + FuelSurcharge
  ├── TotalPrice = SurchargedPrice * PassengerCount
  └── PricePerPassenger = TotalPrice / PassengerCount
     [Round to 2 decimals]

BudgetWings Calculation:
  ├── Discount = BasePrice * 0.10
  ├── DiscountedPrice = BasePrice - Discount
  ├── MinimumPrice = Max(DiscountedPrice, 29.99)
  ├── TotalPrice = MinimumPrice * PassengerCount
  └── PricePerPassenger = TotalPrice / PassengerCount
     [Round to 2 decimals]
```

### Response Structure
```json
"pricing": {
  "baseFare": 150.00,
  "totalPrice": 345.00,
  "pricePerPassenger": 172.50,
  "breakdown": {
    "provider": "GlobalAir",
    "rule": "Base fare + 15% fuel surcharge",
    "calculation": "150 * 1.15 * 2 passengers = 345.00"
  }
}
```

---

## 7. Project Structure (Clean Architecture)

```
SkyRouteProject/
├── Backend/
│   ├── SkyRoute.Api/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── AirportsController.cs
│   │   │   ├── FlightsController.cs
│   │   │   └── BookingsController.cs
│   │   ├── Middleware/
│   │   │   ├── JwtMiddleware.cs
│   │   │   ├── ErrorHandlingMiddleware.cs
│   │   │   └── ValidationMiddleware.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── SkyRoute.Application/
│   │   ├── Services/
│   │   │   ├── AuthService.cs
│   │   │   ├── FlightSearchService.cs
│   │   │   ├── BookingService.cs
│   │   │   └── AirportService.cs
│   │   ├── DTOs/
│   │   │   ├── Auth/
│   │   │   │   ├── RegisterRequestDto.cs
│   │   │   │   ├── LoginRequestDto.cs
│   │   │   │   └── AuthResponseDto.cs
│   │   │   ├── Flight/
│   │   │   │   ├── FlightSearchRequestDto.cs
│   │   │   │   ├── FlightSearchResponseDto.cs
│   │   │   │   ├── FlightDto.cs
│   │   │   │   └── PricingDto.cs
│   │   │   ├── Booking/
│   │   │   │   ├── CreateBookingRequestDto.cs
│   │   │   │   ├── BookingConfirmationDto.cs
│   │   │   │   └── PassengerDetailsDto.cs
│   │   │   └── Airport/
│   │   │       └── AirportDto.cs
│   │   ├── Validators/
│   │   │   ├── FlightSearchValidator.cs
│   │   │   ├── BookingValidator.cs
│   │   │   ├── DocumentNumberValidator.cs
│   │   │   └── EmailValidator.cs
│   │   └── Interfaces/
│   │       ├── IAuthService.cs
│   │       ├── IFlightSearchService.cs
│   │       ├── IBookingService.cs
│   │       └── IAirportService.cs
│   │
│   ├── SkyRoute.Domain/
│   │   ├── Models/
│   │   │   ├── User.cs
│   │   │   ├── Airport.cs
│   │   │   ├── Airline.cs
│   │   │   ├── Flight.cs
│   │   │   ├── Booking.cs
│   │   │   └── PassengerDetail.cs
│   │   ├── Enums/
│   │   │   ├── CabinClassEnum.cs
│   │   │   ├── DocumentTypeEnum.cs
│   │   │   └── BookingStatusEnum.cs
│   │   └── ValueObjects/
│   │       └── PricingResult.cs
│   │
│   ├── SkyRoute.Infrastructure/
│   │   ├── Data/
│   │   │   ├── SkyRouteDbContext.cs
│   │   │   └── Repositories/
│   │   │       ├── FlightRepository.cs
│   │   │       ├── BookingRepository.cs
│   │   │       ├── UserRepository.cs
│   │   │       └── AirportRepository.cs
│   │   ├── Providers/
│   │   │   ├── IFlightProvider.cs
│   │   │   ├── GlobalAirProvider.cs
│   │   │   ├── BudgetWingsProvider.cs
│   │   │   └── FlightProviderFactory.cs
│   │   ├── Pricing/
│   │   │   ├── IFlightPricingStrategy.cs
│   │   │   ├── GlobalAirPricingStrategy.cs
│   │   │   └── BudgetWingsPricingStrategy.cs
│   │   ├── Authentication/
│   │   │   ├── JwtTokenProvider.cs
│   │   │   ├── PasswordHasher.cs
│   │   │   └── JwtSettings.cs
│   │   └── Configurations/
│   │       └── DependencyInjection.cs
│   │
│   ├── SkyRoute.Tests/
│   │   ├── Unit/
│   │   │   ├── Services/
│   │   │   └── Validators/
│   │   ├── Integration/
│   │   │   ├── Controllers/
│   │   │   └── Endpoints/
│   │   └── Fixtures/
│   │       └── TestData.cs
│   │
│   └── SkyRoute.sln
│
├── Frontend/
│   └── [Phase 2]
│
└── docs/
    ├── project-requirements.md
    └── ARCHITECTURE.md
```

---

## 8. Validation Architecture

### Approach: Fluent Validation + Data Annotations

### Global Validation Strategy
```
1. Data Annotations on DTOs (basic validation)
   - [Required]
   - [StringLength]
   - [EmailAddress]
   - [Range]

2. Fluent Validators (complex validation)
   - FlightSearchValidator
   - CreateBookingRequestValidator
   - DocumentNumberValidator
   - RegisterRequestValidator

3. Middleware/Service-level validation
   - Business rule validation (flight exists, date in future)
   - Authorization validation (user owns booking)
```

### Key Validation Rules

**Flight Search:**
- ✓ Origin ≠ Destination
- ✓ Departure date > today
- ✓ Passengers: 1-9
- ✓ Valid cabin class
- ✓ Airports exist

**Booking:**
- ✓ Flight exists and is active
- ✓ Passengers ≤ flight capacity
- ✓ All passenger details provided
- ✓ Valid email format
- ✓ Document type matches route (domestic/international)
- ✓ Document number matches format

**Document Validation:**
```
Domestic Routes (same country):
  - Label: "National ID"
  - Format: 8-12 alphanumeric characters
  - Example: "AB123456"

International Routes (different countries):
  - Label: "Passport Number"
  - Format: 6-9 alphanumeric characters
  - Example: "A12345678"
```

---

## 9. Implementation Order (Phases)

### Phase 1: Core Infrastructure & Database
**Deliverable:** Database schema, DbContext, Models
- [ ] Create SkyRoute.Domain project
- [ ] Define all models (User, Airport, Flight, Booking, etc.)
- [ ] Create SkyRoute.Infrastructure project
- [ ] Set up SkyRouteDbContext with EF Core
- [ ] Create database migrations
- [ ] Seed initial airport data

### Phase 2: Authentication & User Management
**Deliverable:** Auth endpoints, JWT token generation
- [ ] Create IAuthService and AuthService
- [ ] Implement JwtTokenProvider
- [ ] Implement PasswordHasher
- [ ] Create AuthController with /register and /login
- [ ] Create JwtMiddleware for token validation
- [ ] Unit tests for auth logic

### Phase 3: Airport & Flight Data Layer
**Deliverable:** /airports endpoint, flight repository, mock data
- [ ] Create AirportRepository and IFlightRepository
- [ ] Create AirportService with GetAllAirports logic
- [ ] Create AirportsController with GET /airports endpoint
- [ ] Seed mock flights into database for predefined routes
- [ ] Create Flight DTOs

### Phase 4: Provider Abstraction & Pricing
**Deliverable:** Strategy + Factory patterns, pricing calculation
- [ ] Create IFlightPricingStrategy
- [ ] Implement GlobalAirPricingStrategy
- [ ] Implement BudgetWingsPricingStrategy
- [ ] Create IFlightProvider interface
- [ ] Implement GlobalAirProvider (returns flights + applies pricing)
- [ ] Implement BudgetWingsProvider
- [ ] Create FlightProviderFactory
- [ ] Unit tests for pricing strategies

### Phase 5: Flight Search Endpoint
**Deliverable:** POST /flights/search (public)
- [ ] Create FlightSearchValidator
- [ ] Create IFlightSearchService
- [ ] Implement FlightSearchService with multi-provider logic
- [ ] Create FlightSearchRequestDto and FlightSearchResponseDto
- [ ] Create FlightsController with POST /search endpoint
- [ ] Integration tests for search endpoint
- [ ] Test pricing calculations (GlobalAir + BudgetWings)

### Phase 6: Booking Endpoint
**Deliverable:** POST /bookings (authenticated), GET /bookings/{code}
- [ ] Create BookingValidator with document validation
- [ ] Create DocumentNumberValidator (domestic vs international logic)
- [ ] Create IBookingService
- [ ] Implement BookingService with country detection
- [ ] Create CreateBookingRequestDto and BookingConfirmationDto
- [ ] Generate booking reference code (format: SK-YYYYMMDDNNNNN)
- [ ] Create BookingsController with [Authorize] endpoints
- [ ] Integration tests for booking flow

### Phase 7: Testing & Documentation
**Deliverable:** Comprehensive tests, README
- [ ] Unit tests for all services
- [ ] Integration tests for all endpoints
- [ ] Security tests (authorization, validation)
- [ ] Performance tests
- [ ] Create comprehensive README with setup instructions
- [ ] Document API contracts
- [ ] Document architecture decisions

---

## 10. Security Considerations

### Input Validation & Sanitization
- ✓ Validate all inputs at controller entry point
- ✓ Use Fluent Validators for complex rules
- ✓ Reject requests with invalid data immediately (fail-fast)
- ✓ Never trust client-side validation alone

### SQL Injection Prevention
- ✓ Use EF Core with parameterized queries exclusively
- ✓ No string concatenation for queries
- ✓ Use LINQ queries (EF automatically parameterizes)

### JWT Security Practices
- ✓ Use HS256 with strong secret (256-bit minimum)
- ✓ Set short expiration (1 hour)
- ✓ Never expose JWT in URLs (use Authorization header only)
- ✓ Validate token signature on every request
- ✓ Check expiration timestamp

### Authorization
- ✓ Verify user identity from JWT claims
- ✓ Implement owner-only access (users can only view own bookings)
- ✓ Use [Authorize] attribute on protected endpoints
- ✓ No hardcoded admin roles (keep simple as per requirements)

### Error Handling
- ✓ Log errors internally with full details
- ✓ Return generic error messages to clients
- ✓ Never expose stack traces, internal paths, database details
- ✓ Return appropriate HTTP status codes (400, 401, 403, 404, 500)

### CORS Configuration
- ✓ Restrict to allowed origins (localhost:4200 for frontend)
- ✓ Allow credentials in requests
- ✓ Explicitly whitelist allowed headers and methods

### Password Security
- ✓ Hash passwords with PBKDF2/BCrypt/Argon2 (use bcrypt minimum 12 rounds)
- ✓ Use salt per password
- ✓ Never store plain-text passwords
- ✓ Enforce minimum password complexity

### Data Privacy
- ✓ Don't expose sensitive data in error messages
- ✓ Validate document numbers without storing duplicates
- ✓ Use HTTPS in production (enforce in appsettings)
- ✓ Follow data protection principles

---

## Summary: Design Patterns Used

| Pattern | Location | Purpose |
|---------|----------|---------|
| **Strategy** | Pricing strategies (GlobalAir, BudgetWings) | Isolate pricing logic; easy new provider addition |
| **Factory** | FlightProviderFactory | Centralize provider creation; maintain single source of truth |
| **Repository** | *Repository classes | Abstract data access; centralize query logic; testable |
| **Dependency Injection** | Program.cs | Loose coupling; testability; configuration management |
| **Middleware** | JwtMiddleware | Cross-cutting concerns (auth, error handling) |

---

## Milestones

- **Milestone 1:** Database schema + Models ✓
- **Milestone 2:** Authentication + JWT ✓
- **Milestone 3:** Flight Search (public) ✓
- **Milestone 4:** Booking Flow (authenticated) ✓
- **Milestone 5:** Comprehensive Testing ✓
- **Milestone 6:** Documentation + Setup ✓
- **Milestone 7:** Frontend Integration (Phase 2)

