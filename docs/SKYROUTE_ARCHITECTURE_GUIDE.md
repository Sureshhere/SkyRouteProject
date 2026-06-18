# SkyRoute Travel Platform - Architecture & Implementation Guide

## Table of Contents
1. [Project Overview](#project-overview)
2. [Main Features](#main-features)
3. [High-Level Architecture](#high-level-architecture)
4. [Core Components](#core-components)
5. [Flight Search - End-to-End Flow](#flight-search---end-to-end-flow)
6. [Booking - End-to-End Flow](#booking---end-to-end-flow)
7. [Backend Design Patterns](#backend-design-patterns)
8. [Pricing Strategy Pattern](#pricing-strategy-pattern)
9. [Validation Framework](#validation-framework)
10. [Authentication & Security](#authentication--security)
11. [Database Models](#database-models)
12. [Presentation Guide](#presentation-guide)

---

## Project Overview

### What is SkyRoute?
SkyRoute is a **Flight Search and Booking aggregation platform** that allows users to search for flights from multiple airline providers, compare prices, and make bookings.

### Key Purpose
- Aggregate flight data from **multiple airline providers** (GlobalAir, BudgetWings)
- Provide a unified interface for flight search
- Support flight booking with passenger details
- Handle provider-specific pricing rules
- Maintain extensibility for future providers

### Technology Stack
| Layer | Technology |
|-------|-----------|
| **Frontend** | Angular 21, Standalone Components, Signals, Reactive Forms |
| **Backend** | ASP.NET Core Web API, C# |
| **Database** | SQL Server |
| **Authentication** | JWT (JSON Web Tokens) |
| **Validation** | FluentValidation |

---

## Main Features

### 1. **Flight Search** (Public Feature - No Authentication)
Users can search for flights by specifying:
- Origin and destination airports
- Departure date
- Number of passengers (1-9)
- Cabin class (Economy, Business, First Class)

**Output**: List of available flights from all registered providers with:
- Flight details (airline, number, times, duration)
- Total price (for all passengers)
- Per-passenger price
- Pricing information and rules applied

### 2. **Flight Results Sorting** (Client-Side)
Results can be sorted by:
- **Price** (low to high, high to low)
- **Duration** (shortest first)
- **Departure Time**

> **Important**: Sorting happens on the frontend — no additional API calls are made

### 3. **Flight Booking** (Authenticated Feature - Requires JWT)
Users can book a selected flight by providing:
- Flight ID
- Departure date
- Passenger details (name, email, document number)

**Process**:
- Validates passenger count (1-9)
- Determines if route is domestic or international
- Validates document numbers accordingly
- Calculates pricing
- Creates booking with reference code
- Returns confirmation

### 4. **Booking Confirmation**
- Booking reference code
- Flight summary
- Passenger details
- Price breakdown

---

## High-Level Architecture

### System Diagram
```
┌──────────────────────────────────────────────────────────────┐
│                  Angular Frontend (Phase 2)                   │
│    - Flight Search Component                                  │
│    - Flight Results Component (with sorting)                  │
│    - Booking Form Component                                   │
└──────────────────────────────────────────────────────────────┘
                            ↓ (HTTP/HTTPS)
                     API Calls & Responses
                            ↓
┌──────────────────────────────────────────────────────────────┐
│                  ASP.NET Core Web API                         │
├──────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────┐ │
│  │           API Layer (Controllers)                       │ │
│  │  - FlightsController (GET /api/flights/search)         │ │
│  │  - BookingsController (POST /api/bookings)             │ │
│  │  - AuthController (POST /api/auth/register/login)      │ │
│  └─────────────────────────────────────────────────────────┘ │
│                            ↓                                   │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │           Application Layer (Services)                  │ │
│  │  - IFlightSearchService                                │ │
│  │  - IBookingService                                     │ │
│  │  - IAuthService                                        │ │
│  │  - IAirportService                                     │ │
│  └─────────────────────────────────────────────────────────┘ │
│                            ↓                                   │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │      Provider Abstraction Layer (Pricing Strategies)    │ │
│  │  ┌─────────────────────────────────────────────────┐  │ │
│  │  │  IFlightPricingStrategy (Interface)            │  │ │
│  │  │  ├── GlobalAirPricingStrategy                  │  │ │
│  │  │  │   (Base fare + 15% fuel surcharge)         │  │ │
│  │  │  └── BudgetWingsPricingStrategy                │  │ │
│  │  │      (Base fare - 10% discount, min $29.99)   │  │ │
│  │  └─────────────────────────────────────────────────┘  │ │
│  └─────────────────────────────────────────────────────────┘ │
│                            ↓                                   │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │         Data Layer (Repositories)                       │ │
│  │  - IFlightRepository                                   │ │
│  │  - IAirportRepository                                  │ │
│  │  - IBookingRepository                                  │ │
│  │  - IUserRepository                                     │ │
│  └─────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────┘
                            ↓
                    ┌──────────────┐
                    │  SQL Server  │
                    └──────────────┘
                    (Persistent Data)
```

### Architectural Principles Applied
1. **Layered Architecture** - Clear separation of concerns
2. **Dependency Injection** - Loose coupling, easier testing
3. **Interface-Based Design** - Extensibility for new providers
4. **Strategy Pattern** - Provider-specific pricing logic
5. **DTOs** - API contracts separated from domain models
6. **Validation Separation** - Business logic from validation

---

## Core Components

### 1. **Controllers (API Layer)**

#### FlightsController
```
File: Backend/SkyRoute.Api/Controllers/FlightsController.cs

Responsibilities:
- Accept flight search requests
- Validate input using FlightSearchValidator
- Call IFlightSearchService
- Return flight results

Endpoint: POST /api/flights/search
Access: Public (No authentication required)
Input: FlightSearchRequestDto
Output: FlightSearchResponseDto
```

#### BookingsController
```
File: Backend/SkyRoute.Api/Controllers/BookingsController.cs

Responsibilities:
- Accept booking requests (Authenticated)
- Validate input using CreateBookingValidator
- Call IBookingService
- Return booking confirmation
- Handle user context extraction

Endpoint: POST /api/bookings
Access: Private (Requires JWT Token)
Input: CreateBookingRequestDto
Output: BookingConfirmationDto
```

**Key Pattern**: Controllers remain thin
- Only validate requests
- Call service methods
- Return responses
- All business logic → Services

---

### 2. **Services (Business Logic Layer)**

#### IFlightSearchService
```csharp
public interface IFlightSearchService
{
    Task<FlightSearchResponseDto> SearchFlightsAsync(FlightSearchRequestDto request);
}
```

**Implementation: FlightSearchService**

| Responsibility | Details |
|---|---|
| **Airport Validation** | Verifies origin & destination exist |
| **Flight Retrieval** | Gets flights by route and cabin class |
| **Provider Integration** | Iterates through all pricing strategies |
| **Price Calculation** | Each provider calculates its own pricing |
| **Result Mapping** | Transforms flights into DTOs |
| **Return Response** | Combined results from all providers |

---

#### IBookingService
```csharp
public interface IBookingService
{
    Task<BookingConfirmationDto> CreateBookingAsync(CreateBookingRequestDto request, Guid userId);
    Task<BookingConfirmationDto> GetBookingByReferenceAsync(string referenceCode, Guid userId);
}
```

**Implementation: BookingService**

| Responsibility | Details |
|---|---|
| **Flight Verification** | Validates flight exists |
| **Passenger Validation** | Checks count is 1-9 |
| **Document Validation** | Determines if domestic/intl, validates ID format |
| **Price Calculation** | Uses provider's pricing strategy |
| **Booking Creation** | Creates booking record with reference code |
| **Passenger Details** | Saves passenger information |
| **Confirmation** | Returns booking confirmation |

---

### 3. **Repositories (Data Access Layer)**

```csharp
public interface IFlightRepository
{
    Task<Flight?> GetByIdAsync(Guid id);
    Task<IEnumerable<Flight>> GetByRouteAndCabinAsync(Guid originId, Guid destinationId, CabinClass cabin);
}

public interface IAirportRepository
{
    Task<Airport?> GetByCodeAsync(string code);
}

public interface IBookingRepository
{
    Task AddAsync(Booking booking);
    Task SaveChangesAsync();
    Task<Booking?> GetByReferenceCodeAsync(string code);
}
```

**Purpose**: Abstraction over database operations using Entity Framework Core

---

### 4. **DTOs (Data Transfer Objects)**

DTOs define the API contract between frontend and backend.

#### Flight Search Request
```csharp
public class FlightSearchRequestDto
{
    public string OriginAirportCode { get; set; }        // "NYC"
    public string DestinationAirportCode { get; set; }   // "LAX"
    public DateOnly DepartureDate { get; set; }          // 2026-07-15
    public int NumberOfPassengers { get; set; }          // 1-9
    public CabinClass CabinClass { get; set; }           // Economy, Business, FirstClass
}
```

#### Flight Search Response
```csharp
public class FlightSearchResponseDto
{
    public IEnumerable<FlightResultDto> Flights { get; set; }
    public SearchMetadataDto SearchMetadata { get; set; }
}

public class FlightResultDto
{
    public Guid Id { get; set; }
    public string AirlineName { get; set; }              // "GlobalAir"
    public string FlightNumber { get; set; }             // "GA123"
    public string OriginCode { get; set; }               // "NYC"
    public string DestinationCode { get; set; }          // "LAX"
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public int DurationMinutes { get; set; }
    public string CabinClass { get; set; }
    public PricingDto Pricing { get; set; }              // Price info
}

public class PricingDto
{
    public decimal BaseFare { get; set; }                // Original price
    public decimal PricePerPassenger { get; set; }       // After calculations
    public decimal TotalPrice { get; set; }              // For all passengers
    public string PricingRule { get; set; }              // "Base fare + 15% fuel surcharge"
}
```

#### Booking Request
```csharp
public class CreateBookingRequestDto
{
    public Guid FlightId { get; set; }
    public DateOnly DepartureDate { get; set; }
    public List<PassengerInputDto> Passengers { get; set; }
}

public class PassengerInputDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string DocumentNumber { get; set; }           // National ID or Passport
}
```

#### Booking Response
```csharp
public class BookingConfirmationDto
{
    public string BookingReferenceCode { get; set; }     // Auto-generated
    public FlightSummaryDto FlightSummary { get; set; }
    public List<PassengerConfirmationDto> Passengers { get; set; }
    public PriceBreakdownDto PriceBreakdown { get; set; }
    public DateTime BookedAt { get; set; }
}
```

---

## Flight Search - End-to-End Flow

### Step-by-Step Walkthrough

#### **1. Frontend Initiates Search**
```javascript
// Angular Component
searchFlights() {
  const request = {
    originAirportCode: "NYC",
    destinationAirportCode: "LAX",
    departureDate: new Date(2026, 6, 15),
    numberOfPassengers: 2,
    cabinClass: "Economy"
  };
  
  this.flightService.searchFlights(request).subscribe(response => {
    this.flights = response.flights;  // Store results
    // Results now sorted client-side (no API call)
  });
}
```

#### **2. FlightsController Receives Request**
```
Endpoint: POST /api/flights/search
Request: {originAirportCode: "NYC", destinationAirportCode: "LAX", ...}

Controller Steps:
├─ Validate request using FlightSearchValidator
├─ Check: Origin/Destination codes are valid
├─ Check: Departure date is not in past
├─ Check: Passengers are 1-9
└─ If valid → Call IFlightSearchService.SearchFlightsAsync()
  If invalid → Return 400 Bad Request
```

#### **3. FlightSearchService Processes**
```
Service: FlightSearchService.SearchFlightsAsync()

Steps:
1. Retrieve origin airport from database
   - If not found → Throw AppException (404)
   
2. Retrieve destination airport from database
   - If not found → Throw AppException (404)
   
3. Get all flights matching:
   - Origin: NYC
   - Destination: LAX
   - Cabin Class: Economy
   
4. For each flight found:
   ├─ Get flight's airline (e.g., "GlobalAir")
   ├─ Find matching pricing strategy
   │  └─ GlobalAirPricingStrategy.CalculatePrice(baseFare=150, passengers=2)
   ├─ Create FlightResultDto with:
   │  ├─ Flight details (number, times, duration)
   │  ├─ Pricing from strategy
   │  └─ Cabin class
   └─ Add to results list
   
5. Return FlightSearchResponseDto containing:
   ├─ List of all flights with prices
   └─ Search metadata (route, date, passengers, results count)
```

#### **4. Pricing Strategy Calculation (GlobalAir Example)**
```
GlobalAir Pricing Rule: Base fare + 15% fuel surcharge

Input:
  BaseFare = $150.00
  NumberOfPassengers = 2

Calculation:
  PricePerPassenger = $150.00 × 1.15 = $172.50
  TotalPrice = $172.50 × 2 = $345.00

Output PricingDto:
{
  BaseFare: 150.00,
  PricePerPassenger: 172.50,
  TotalPrice: 345.00,
  PricingRule: "Base fare + 15% fuel surcharge"
}
```

#### **5. Pricing Strategy Calculation (BudgetWings Example)**
```
BudgetWings Pricing Rule: Base fare - 10% discount (minimum $29.99)

Input:
  BaseFare = $150.00
  NumberOfPassengers = 2

Calculation:
  Discounted = $150.00 × 0.90 = $135.00
  PricePerPassenger = Max($135.00, $29.99) = $135.00
  TotalPrice = $135.00 × 2 = $270.00

Output PricingDto:
{
  BaseFare: 150.00,
  PricePerPassenger: 135.00,
  TotalPrice: 270.00,
  PricingRule: "Base fare - 10% discount (minimum $29.99)"
}

Note: Discount only applied to base fare, not to minimum price floor
```

#### **6. Frontend Receives Response**
```javascript
// API Response Example
{
  flights: [
    {
      id: "guid-1",
      airlineName: "GlobalAir",
      flightNumber: "GA123",
      originCode: "NYC",
      destinationCode: "LAX",
      departureTime: "2026-07-15T09:00:00",
      arrivalTime: "2026-07-15T12:30:00",
      durationMinutes: 210,
      cabinClass: "Economy",
      pricing: {
        baseFare: 150.00,
        pricePerPassenger: 172.50,
        totalPrice: 345.00,
        pricingRule: "Base fare + 15% fuel surcharge"
      }
    },
    {
      id: "guid-2",
      airlineName: "BudgetWings",
      flightNumber: "BW456",
      originCode: "NYC",
      destinationCode: "LAX",
      departureTime: "2026-07-15T14:00:00",
      arrivalTime: "2026-07-15T17:45:00",
      durationMinutes: 225,
      cabinClass: "Economy",
      pricing: {
        baseFare: 150.00,
        pricePerPassenger: 135.00,
        totalPrice: 270.00,
        pricingRule: "Base fare - 10% discount (minimum $29.99)"
      }
    }
  ],
  searchMetadata: {
    origin: "NYC",
    destination: "LAX",
    departureDate: "2026-07-15",
    passengers: 2,
    cabinClass: "Economy",
    resultsCount: 2
  }
}
```

#### **7. Frontend Displays & Sorts Results**
```
User sees in UI:
┌────────────────────────────────────────────────────────────┐
│ NYC → LAX | 2 Passengers | Jul 15, 2026 | Economy          │
├────────────────────────────────────────────────────────────┤
│ GlobalAir GA123   │ 09:00 → 12:30 │ 3h 30m │ USD 345.00   │
│                   │               │        │ per person: 172.50 │
├────────────────────────────────────────────────────────────┤
│ BudgetWings BW456 │ 14:00 → 17:45 │ 3h 45m │ USD 270.00   │
│                   │               │        │ per person: 135.00 │
└────────────────────────────────────────────────────────────┘

User clicks "Sort by Price (Low to High)"
→ Frontend sorts array by pricing.totalPrice
→ NO API CALL (client-side only)
```

---

## Booking - End-to-End Flow

### Step-by-Step Walkthrough

#### **1. User Selects Flight & Provides Details**
```javascript
// Angular Booking Component
submitBooking() {
  const bookingRequest = {
    flightId: "guid-1",
    departureDate: new Date(2026, 6, 15),
    passengers: [
      {
        fullName: "John Doe",
        email: "john@example.com",
        documentNumber: "12345678"      // National ID or Passport
      },
      {
        fullName: "Jane Smith",
        email: "jane@example.com",
        documentNumber: "87654321"
      }
    ]
  };
  
  // Include JWT token in Authorization header
  this.bookingService.createBooking(bookingRequest).subscribe(...);
}
```

#### **2. BookingsController Receives Request**
```
Endpoint: POST /api/bookings
Authorization: Bearer {JWT_TOKEN}

Controller Steps:
├─ Verify JWT token is valid (Middleware)
├─ Extract userId from JWT claims
├─ Validate request using CreateBookingValidator
│  ├─ Check: FlightId is GUID
│  ├─ Check: DepartureDate is valid
│  └─ Check: Passengers array has 1-9 entries
├─ If valid → Call IBookingService.CreateBookingAsync()
└─ If invalid → Return 400 Bad Request
```

#### **3. BookingService Validates & Creates Booking**

```
Service: BookingService.CreateBookingAsync()

Step 1: Flight Validation
├─ Get flight by FlightId
├─ If not found → Throw AppException (404)
└─ Retrieve flight relationships (airline, airports)

Step 2: Passenger Count Validation
├─ Check: passengers.Count is between 1 and 9
└─ If not → Throw AppException

Step 3: Determine Route Type (Domestic vs International)
├─ Get origin airport country code
├─ Get destination airport country code
├─ Compare: If same country → Domestic, else → International
└─ Set documentType accordingly

Step 4: Validate Each Passenger's Document
├─ For each passenger:
│  ├─ Determine required format based on route type
│  ├─ If Domestic:
│  │  └─ Validate National ID (8-12 alphanumeric)
│  └─ If International:
│     └─ Validate Passport Number (6-9 alphanumeric)
│  └─ If invalid → Throw AppException with specific format
└─ All passengers validated

Step 5: Calculate Price
├─ Get pricing strategy for flight's airline
│  └─ Example: For "GlobalAir" → GlobalAirPricingStrategy
├─ Call strategy.CalculatePrice(baseFare, passengerCount)
└─ Receive pricing with totalPrice & pricePerPassenger

Step 6: Generate Booking Reference Code
├─ Create unique reference code
│  └─ Format: "SKY" + timestamp + random (e.g., "SKY20260715AB12CD")
└─ Ensure uniqueness in database

Step 7: Create Booking Domain Model
├─ Instantiate Booking object with:
│  ├─ Id: New GUID
│  ├─ BookingReferenceCode: Generated code
│  ├─ UserId: From JWT token
│  ├─ FlightId: From request
│  ├─ DepartureDate: Calculated from DateOnly + Flight time
│  ├─ NumberOfPassengers: Count
│  ├─ TotalPrice: From pricing strategy
│  ├─ PricePerPassenger: From pricing strategy
│  ├─ Status: BookingStatus.Confirmed
│  ├─ CreatedAt: DateTime.UtcNow
│  └─ PassengerDetails: List of passenger records
└─ Add PassengerDetail for each passenger

Step 8: Persist to Database
├─ Call _bookingRepository.AddAsync(booking)
└─ Call _bookingRepository.SaveChangesAsync()

Step 9: Return Confirmation
└─ Map Booking model to BookingConfirmationDto
```

#### **4. Document Validation Logic**

```
Domestic vs International Detection:

if (originAirport.CountryCode == destinationAirport.CountryCode)
   → DOMESTIC ROUTE
   → Required Document: National ID (8-12 alphanumeric)
else
   → INTERNATIONAL ROUTE
   → Required Document: Passport Number (6-9 alphanumeric)

Examples:
┌────────────────────────────────────────────────────────┐
│ Route: NYC (US) → LAX (US)   → DOMESTIC                │
│ Required: National ID (12345678)                       │
├────────────────────────────────────────────────────────┤
│ Route: NYC (US) → LHR (UK)   → INTERNATIONAL           │
│ Required: Passport (A12345678)                         │
├────────────────────────────────────────────────────────┤
│ Route: MEX (MX) → CUN (MX)   → DOMESTIC                │
│ Required: National ID (ABCD1234)                       │
└────────────────────────────────────────────────────────┘
```

#### **5. Backend Returns Booking Confirmation**

```json
{
  "bookingReferenceCode": "SKY20260715AB12CD",
  "flightSummary": {
    "airline": "GlobalAir",
    "flightNumber": "GA123",
    "route": "NYC → LAX",
    "departureTime": "2026-07-15T09:00:00Z",
    "arrivalTime": "2026-07-15T12:30:00Z",
    "cabinClass": "Economy"
  },
  "passengers": [
    {
      "fullName": "John Doe",
      "email": "john@example.com",
      "documentType": "NationalID",
      "documentNumber": "12345678"
    },
    {
      "fullName": "Jane Smith",
      "email": "jane@example.com",
      "documentType": "NationalID",
      "documentNumber": "87654321"
    }
  ],
  "priceBreakdown": {
    "baseFare": 150.00,
    "pricePerPassenger": 172.50,
    "numberOfPassengers": 2,
    "totalPrice": 345.00,
    "pricingRule": "Base fare + 15% fuel surcharge"
  },
  "bookedAt": "2026-07-13T10:30:00Z"
}
```

#### **6. Frontend Displays Confirmation**
```
User sees in UI:
┌────────────────────────────────────────────────────────┐
│ ✓ BOOKING CONFIRMED                                    │
├────────────────────────────────────────────────────────┤
│ Reference Code: SKY20260715AB12CD                      │
├────────────────────────────────────────────────────────┤
│ Flight: GlobalAir GA123                                │
│ Route:  NYC → LAX                                      │
│ Time:   Jul 15, 2026 | 09:00 → 12:30                 │
├────────────────────────────────────────────────────────┤
│ Passengers:                                            │
│ 1. John Doe (ID: 12345678)                            │
│ 2. Jane Smith (ID: 87654321)                          │
├────────────────────────────────────────────────────────┤
│ Price Breakdown:                                       │
│ Base Fare:         $150.00 × 2 = $300.00              │
│ Fuel Surcharge:    15%           = $45.00              │
│ Total:                           $345.00               │
│ Per Person:                      $172.50               │
└────────────────────────────────────────────────────────┘
```

---

## Backend Design Patterns

### 1. **Layered Architecture**
```
┌─────────────────────┐
│   API Layer         │ ← Request/Response handling
├─────────────────────┤
│ Application Layer   │ ← Business logic
├─────────────────────┤
│ Domain Layer        │ ← Core models & concepts
├─────────────────────┤
│ Infrastructure Layer│ ← Database, external services
└─────────────────────┘
```

**Benefits**:
- Clear separation of concerns
- Easy to test each layer independently
- Maintainable and readable code
- Ability to swap implementations (e.g., different database)

---

### 2. **Dependency Injection**
```csharp
// In Program.cs / DependencyInjection.cs
services.AddScoped<IFlightSearchService, FlightSearchService>();
services.AddScoped<IFlightRepository, FlightRepository>();

// In controller
public FlightsController(
    IFlightSearchService flightSearchService,
    IValidator<FlightSearchRequestDto> searchValidator)
{
    _flightSearchService = flightSearchService;
    _searchValidator = searchValidator;
}
```

**Why DI?**
- Loose coupling between components
- Easy to swap implementations for testing
- Centralized configuration of dependencies
- Supports testing with mock implementations

---

### 3. **Repository Pattern**
```csharp
public interface IFlightRepository
{
    Task<Flight?> GetByIdAsync(Guid id);
    Task<IEnumerable<Flight>> GetByRouteAndCabinAsync(Guid originId, Guid destinationId, CabinClass cabin);
    Task AddAsync(Flight flight);
    Task SaveChangesAsync();
}
```

**Purpose**:
- Abstract data access logic
- Provide a consistent interface for database operations
- Easy to test with mock repositories
- Database implementation can change without affecting business logic

---

### 4. **Service Pattern**
```csharp
public interface IFlightSearchService
{
    Task<FlightSearchResponseDto> SearchFlightsAsync(FlightSearchRequestDto request);
}

public class FlightSearchService : IFlightSearchService
{
    public async Task<FlightSearchResponseDto> SearchFlightsAsync(FlightSearchRequestDto request)
    {
        // Orchestrate: validate → fetch → calculate → return
    }
}
```

**Responsibilities**:
- Coordinate between repositories
- Implement business logic
- Handle validation
- Transform data (models → DTOs)

---

## Pricing Strategy Pattern

### Why Strategy Pattern?

Each airline has **different pricing rules**:
- **GlobalAir**: Base + 15% surcharge
- **BudgetWings**: Base - 10% discount (min $29.99)
- **Future Provider**: Different rules

**Without Strategy Pattern** (Tightly Coupled):
```csharp
public decimal CalculatePrice(Flight flight, int passengers)
{
    if (flight.Airline.Name == "GlobalAir")
        return flight.BaseFare * 1.15m * passengers;
    else if (flight.Airline.Name == "BudgetWings")
        return Math.Max(flight.BaseFare * 0.90m, 29.99m) * passengers;
    else if (flight.Airline.Name == "NewProvider")
        // ... add another condition
}
// Problems: grows endlessly, hard to test, violates Open/Closed Principle
```

**With Strategy Pattern** (Extensible):
```csharp
public interface IFlightPricingStrategy
{
    string ProviderName { get; }
    PricingResult CalculatePrice(decimal baseFare, int numberOfPassengers);
}

public class GlobalAirPricingStrategy : IFlightPricingStrategy
{
    public string ProviderName => "GlobalAir";
    public PricingResult CalculatePrice(decimal baseFare, int numberOfPassengers)
    {
        var pricePerPassenger = Math.Round(baseFare * 1.15m, 2);
        var totalPrice = Math.Round(pricePerPassenger * numberOfPassengers, 2);
        return new PricingResult
        {
            BaseFare = baseFare,
            PricePerPassenger = pricePerPassenger,
            TotalPrice = totalPrice,
            PricingRule = "Base fare + 15% fuel surcharge"
        };
    }
}

public class BudgetWingsPricingStrategy : IFlightPricingStrategy
{
    private const decimal MinimumPrice = 29.99m;
    public string ProviderName => "BudgetWings";
    public PricingResult CalculatePrice(decimal baseFare, int numberOfPassengers)
    {
        var discounted = baseFare - (baseFare * 0.10m);
        var pricePerPassenger = Math.Round(Math.Max(discounted, MinimumPrice), 2);
        var totalPrice = Math.Round(pricePerPassenger * numberOfPassengers, 2);
        return new PricingResult
        {
            BaseFare = baseFare,
            PricePerPassenger = pricePerPassenger,
            TotalPrice = totalPrice,
            PricingRule = $"Base fare - 10% discount (minimum ${MinimumPrice})"
        };
    }
}
```

### How to Add a New Provider

**Step 1**: Create new strategy class
```csharp
public class PremiumAirlinesPricingStrategy : IFlightPricingStrategy
{
    public string ProviderName => "PremiumAirlines";
    
    public PricingResult CalculatePrice(decimal baseFare, int numberOfPassengers)
    {
        // Implement PremiumAirlines-specific logic
        // Example: Base + 20% premium + 5% per-passenger fee
        var premium = baseFare * 1.20m + (numberOfPassengers * 5m);
        return new PricingResult { ... };
    }
}
```

**Step 2**: Register in DI (Program.cs)
```csharp
services.AddSingleton<IFlightPricingStrategy, PremiumAirlinesPricingStrategy>();
```

**Step 3**: That's it! The service will automatically find and use it.

---

## Validation Framework

### Validation Hierarchy

```
1. Input Validation (Controllers)
   └─ Request object structure, data types

2. Business Logic Validation (Services)
   └─ Business rules, consistency checks

3. Domain Validation (Domain Models)
   └─ Entity invariants
```

### FluentValidation Integration

```csharp
public class FlightSearchValidator : AbstractValidator<FlightSearchRequestDto>
{
    public FlightSearchValidator()
    {
        RuleFor(x => x.OriginAirportCode)
            .NotEmpty().WithMessage("Origin airport is required")
            .Length(1, 10).WithMessage("Airport code must be 1-10 characters");

        RuleFor(x => x.NumberOfPassengers)
            .InclusiveBetween(1, 9).WithMessage("Passengers must be 1-9");

        RuleFor(x => x.DepartureDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("Departure date must be today or later");
    }
}
```

**Usage in Controller**:
```csharp
[HttpPost("search")]
public async Task<IActionResult> Search([FromBody] FlightSearchRequestDto request)
{
    var validation = await _searchValidator.ValidateAsync(request);
    if (!validation.IsValid)
        return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

    var result = await _flightSearchService.SearchFlightsAsync(request);
    return Ok(result);
}
```

### Document Number Validation

```csharp
public static class DocumentNumberValidator
{
    // Domestic: 8-12 alphanumeric
    public static bool IsValidNationalId(string id)
        => !string.IsNullOrWhiteSpace(id) && id.Length >= 8 && id.Length <= 12;

    // International: 6-9 alphanumeric
    public static bool IsValidPassport(string passport)
        => !string.IsNullOrWhiteSpace(passport) && passport.Length >= 6 && passport.Length <= 9;

    // Determine if route is domestic
    public static bool IsDomesticRoute(string originCountry, string destinationCountry)
        => originCountry?.Equals(destinationCountry, StringComparison.OrdinalIgnoreCase) == true;
}
```

**Usage in Booking Service**:
```csharp
var isDomestic = DocumentNumberValidator.IsDomesticRoute(
    flight.OriginAirport.CountryCode,
    flight.DestinationAirport.CountryCode);

var isValid = isDomestic
    ? DocumentNumberValidator.IsValidNationalId(passenger.DocumentNumber)
    : DocumentNumberValidator.IsValidPassport(passenger.DocumentNumber);
```

---

## Authentication & Security

### JWT (JSON Web Token) Authentication

```
┌─────────────────────────────────────────────────────────┐
│ User Login Process                                      │
├─────────────────────────────────────────────────────────┤
│ 1. User submits: email + password                       │
│ 2. AuthService validates credentials                    │
│ 3. If valid → JwtTokenProvider generates JWT            │
│ 4. Return token to frontend                             │
│ 5. Frontend stores token (localStorage/cookie)          │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ Protected Request (e.g., Booking)                       │
├─────────────────────────────────────────────────────────┤
│ 1. Frontend includes: Authorization: Bearer {token}     │
│ 2. Middleware validates token signature                 │
│ 3. Middleware extracts userId from claims              │
│ 4. Service receives authenticated userId                │
│ 5. Booking is created for that user                     │
└─────────────────────────────────────────────────────────┘
```

### JWT Claims

```csharp
public class JwtTokenProvider : IJwtTokenProvider
{
    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),  // userId
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("sub", user.Id.ToString())  // Standard "sub" claim
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),  // 7-day expiration
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### Endpoint Protection

```csharp
// Public endpoint (No JWT required)
[HttpPost("search")]
public async Task<IActionResult> Search([FromBody] FlightSearchRequestDto request)
{
    // Anyone can search flights
}

// Protected endpoint (JWT Required)
[HttpPost]
[Authorize]  // ← This enforces JWT validation
public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto request)
{
    var userId = GetCurrentUserId();  // Extract from JWT claims
    var result = await _bookingService.CreateBookingAsync(request, userId);
    return CreatedAtAction(nameof(GetByReference), result);
}
```

---

## Database Models

### Entity Relationships

```
Users
├─ 1:M → Bookings
│       └─ M:1 → Flights
│               └─ M:1 → Airlines
│               └─ M:1 → Airports (Origin)
│               └─ M:1 → Airports (Destination)
│       └─ 1:M → PassengerDetails

Airports
├─ 1:M → Flights (as Origin)
└─ 1:M → Flights (as Destination)

Airlines
└─ 1:M → Flights
```

### Core Entities

#### User
```csharp
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }                // Unique
    public string PasswordHash { get; set; }         // Hashed
    public string PasswordSalt { get; set; }         // For hashing
    public string FullName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    
    public ICollection<Booking> Bookings { get; set; }
}
```

#### Airport
```csharp
public class Airport
{
    public Guid Id { get; set; }
    public string Code { get; set; }                 // "NYC", "LAX"
    public string Name { get; set; }                 // "LaGuardia"
    public string Country { get; set; }              // "United States"
    public string CountryCode { get; set; }          // "US"
    public string City { get; set; }                 // "New York"
    public bool IsActive { get; set; }
    
    public ICollection<Flight> OriginFlights { get; set; }
    public ICollection<Flight> DestinationFlights { get; set; }
}
```

#### Airline
```csharp
public class Airline
{
    public Guid Id { get; set; }
    public string Name { get; set; }                 // "GlobalAir", "BudgetWings"
    public string Code { get; set; }                 // "GA", "BW"
    public bool IsActive { get; set; }
    
    public ICollection<Flight> Flights { get; set; }
}
```

#### Flight
```csharp
public class Flight
{
    public Guid Id { get; set; }
    public Guid AirlineId { get; set; }              // FK
    public string FlightNumber { get; set; }         // "GA123"
    public Guid OriginAirportId { get; set; }        // FK
    public Guid DestinationAirportId { get; set; }   // FK
    public TimeSpan DepartureTime { get; set; }      // HH:MM:SS
    public TimeSpan ArrivalTime { get; set; }        // HH:MM:SS
    public int DurationMinutes { get; set; }
    public CabinClass CabinClass { get; set; }       // Economy, Business, FirstClass
    public decimal BaseFare { get; set; }            // $150.00
    public bool IsActive { get; set; }
    
    public Airline Airline { get; set; }
    public Airport OriginAirport { get; set; }
    public Airport DestinationAirport { get; set; }
    public ICollection<Booking> Bookings { get; set; }
}
```

#### Booking
```csharp
public class Booking
{
    public Guid Id { get; set; }
    public string BookingReferenceCode { get; set; } // "SKY20260715AB12CD"
    public Guid UserId { get; set; }                 // FK
    public Guid FlightId { get; set; }               // FK
    public DateTime DepartureDate { get; set; }
    public int NumberOfPassengers { get; set; }      // 1-9
    public decimal TotalPrice { get; set; }          // $345.00
    public decimal PricePerPassenger { get; set; }   // $172.50
    public BookingStatus Status { get; set; }        // Confirmed, Cancelled
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public User User { get; set; }
    public Flight Flight { get; set; }
    public ICollection<PassengerDetail> PassengerDetails { get; set; }
}
```

#### PassengerDetail
```csharp
public class PassengerDetail
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }              // FK
    public string FullName { get; set; }
    public string Email { get; set; }
    public DocumentType DocumentType { get; set; }   // NationalID, PassportNumber
    public string DocumentNumber { get; set; }       // "12345678"
    public int PassengerIndex { get; set; }          // 1, 2, 3, ...
    public DateTime CreatedAt { get; set; }
    
    public Booking Booking { get; set; }
}
```

---

## Presentation Guide

### How to Explain SkyRoute to Someone

#### **Part 1: Start with the Big Picture (5 minutes)**

> "SkyRoute is a flight aggregation platform. Imagine you want to compare prices from multiple airlines to book a flight. Instead of visiting each airline's website, you search once, and SkyRoute shows you results from GlobalAir, BudgetWings, and any other providers we integrate."

**Show this diagram**:
```
User → SkyRoute → Multiple Airlines → Results
                      ↓
              Each airline has different prices
```

#### **Part 2: The Two Main Workflows (10 minutes)**

##### **Workflow 1: Flight Search**
```
1. User enters: "NYC to LAX, 2 people, July 15, Economy"
2. Backend validates the airports exist
3. Backend finds all flights for that route & cabin
4. Backend asks GlobalAir: "What's your price for 2 passengers?"
5. GlobalAir: "Base $150, add 15% surcharge = $172.50 per person"
6. Backend asks BudgetWings: "What's your price?"
7. BudgetWings: "Base $150, minus 10% = $135.00 per person"
8. Backend sends both options to frontend
9. Frontend displays results, user can sort by price/time

Key Point: Different airlines have different pricing rules
```

##### **Workflow 2: Flight Booking**
```
1. User selects GlobalAir flight + provides passenger info
2. User must log in (JWT authentication)
3. Backend determines if route is domestic or international
   - NYC → LAX = Domestic → Need National ID
   - NYC → London = International → Need Passport
4. Backend validates document numbers match the format
5. Backend calculates final price using GlobalAir's pricing rule
6. Backend saves booking to database
7. Backend generates booking reference: "SKY20260715AB12CD"
8. User gets confirmation with all details

Key Point: Validation is route-aware
```

#### **Part 3: The Architecture (5 minutes)**

> "The backend is organized into layers:"

```
Controllers (API Entry Points)
    ↓
Services (Business Logic)
    ↓
Repositories (Database Access)
    ↓
Database
```

| Layer | Example | Responsibility |
|-------|---------|---|
| **Controller** | FlightsController | Accept request, validate, call service |
| **Service** | FlightSearchService | Coordinate all the pieces, calculate prices |
| **Repository** | FlightRepository | Get data from database |
| **Database** | SQL Server | Store everything |

#### **Part 4: The Smart Part - Pricing Strategies (5 minutes)**

> "The genius of the design is how we handle different airline pricing:"

**Show the problem**:
```csharp
// BAD: Hard to maintain
if (airline == "GlobalAir") {
    price = baseFare * 1.15;
} else if (airline == "BudgetWings") {
    price = baseFare * 0.90;  // min $29.99
} else if (airline == "NewProvider") {
    // ... keep adding conditions
}
// This grows forever!
```

**Show the solution**:
```csharp
// GOOD: Easy to extend
interface IFlightPricingStrategy {
    decimal Calculate(decimal baseFare, int passengers);
}

class GlobalAirPricingStrategy : IFlightPricingStrategy {
    // Base + 15%
}

class BudgetWingsPricingStrategy : IFlightPricingStrategy {
    // Base - 10% (min $29.99)
}

// To add new provider:
class PremiumAirlinesPricingStrategy : IFlightPricingStrategy {
    // Custom logic
}
```

> "Each airline implements its own pricing logic. To add a new airline, we just add a new strategy. No need to change existing code."

#### **Part 5: Validation & Security (3 minutes)**

**Validation**:
```
Domestic Route (NYC → LAX):
├─ Required Document: National ID
├─ Format: 8-12 alphanumeric
└─ Example: "12345678"

International Route (NYC → London):
├─ Required Document: Passport
├─ Format: 6-9 alphanumeric
└─ Example: "A12345678"
```

**Security**:
```
Public Endpoints:
├─ POST /api/flights/search (anyone can search)
└─ POST /api/auth/register (anyone can register)

Protected Endpoints:
├─ POST /api/bookings (must have JWT token)
└─ GET /api/bookings/{ref} (must be logged in)
```

#### **Part 6: Key Concepts Summary (2 minutes)**

| Concept | Why It Matters |
|---------|---|
| **Layered Architecture** | Keeps code organized, easy to test |
| **Dependency Injection** | Loose coupling, easy to swap implementations |
| **Strategy Pattern** | Extensible pricing for new airlines |
| **Repository Pattern** | Abstract data access, easy to change database |
| **DTOs** | Clear API contracts |
| **Validation** | Business rules enforced consistently |
| **JWT Authentication** | Secure, stateless authentication |

---

### Talking Points for Different Audiences

#### **For Business Stakeholders**
- "We built a scalable platform that supports multiple airlines"
- "Adding new airlines is simple - no risky code changes"
- "Clear separation of concerns makes maintenance cheap"
- "Validation rules are consistent across the application"

#### **For Technical Interviews**
- "I used the Strategy Pattern for provider-specific pricing"
- "Clean architecture with clear separation of concerns"
- "DTOs for API contracts, domain models for business logic"
- "Dependency injection for testability and loose coupling"
- "FluentValidation for centralized, maintainable validation"
- "JWT authentication for secure, stateless authentication"

#### **For New Team Members**
- "The codebase is organized by layers"
- "Each service handles one business capability"
- "Adding a feature? Follow this pattern..."
- "Here's where pricing logic lives, validation logic, etc."

---

## Quick Reference: Adding a New Feature

### Scenario: Add Support for a New Airline "SkyMax Airlines"

#### Step 1: Create Pricing Strategy
**File**: `Backend/SkyRoute.Infrastructure/Pricing/SkyMaxPricingStrategy.cs`
```csharp
public class SkyMaxPricingStrategy : IFlightPricingStrategy
{
    public string ProviderName => "SkyMax";

    public PricingResult CalculatePrice(decimal baseFare, int numberOfPassengers)
    {
        // SkyMax pricing: Base + 12% surcharge + $5 booking fee per passenger
        var surcharge = baseFare * 0.12m;
        var bookingFee = 5m * numberOfPassengers;
        var pricePerPassenger = Math.Round((baseFare + surcharge + (bookingFee / numberOfPassengers)), 2);
        var totalPrice = Math.Round(pricePerPassenger * numberOfPassengers, 2);

        return new PricingResult
        {
            BaseFare = baseFare,
            PricePerPassenger = pricePerPassenger,
            TotalPrice = totalPrice,
            PricingRule = "Base fare + 12% surcharge + $5 booking fee per passenger"
        };
    }
}
```

#### Step 2: Register in Dependency Injection
**File**: `Backend/SkyRoute.Infrastructure/Configurations/DependencyInjection.cs`
```csharp
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    // ... existing code ...

    // Pricing Strategies
    services.AddSingleton<IFlightPricingStrategy, GlobalAirPricingStrategy>();
    services.AddSingleton<IFlightPricingStrategy, BudgetWingsPricingStrategy>();
    services.AddSingleton<IFlightPricingStrategy, SkyMaxPricingStrategy>();  // ← Add this line

    return services;
}
```

#### Step 3: Done!
- Create flights for SkyMax in database with BaseFare
- When FlightSearchService runs, it automatically finds and uses SkyMaxPricingStrategy
- No other code changes needed!

---

## Conclusion

### What Makes SkyRoute Well-Designed

1. **Extensibility**: Adding new airlines requires adding one class + one DI registration
2. **Maintainability**: Clear layers, each with a specific responsibility
3. **Testability**: Dependency injection makes it easy to mock dependencies
4. **Security**: Authentication built in, validation centralized
5. **Clarity**: DTOs define API contracts, services orchestrate, repositories access data

### The Development Philosophy

> **"Build only what is needed. Keep it simple. Make it maintainable."**

Every architectural decision supports this. Nothing is over-engineered. But everything is flexible enough to support future growth.

---

**Document Version**: 1.0  
**Last Updated**: July 2026  
**For Questions**: Refer to code comments and project requirements document
