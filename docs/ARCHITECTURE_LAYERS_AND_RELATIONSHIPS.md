# SkyRoute Architecture: Layers, Classes & Interface Relationships

**Purpose:** Detailed technical explanation of application architecture, component relationships, and data flow through each layer.

---

## TABLE OF CONTENTS

1. [Architecture Overview](#architecture-overview)
2. [Layered Architecture](#layered-architecture)
3. [Layer Details & Responsibilities](#layer-details--responsibilities)
4. [Interface Relationships](#interface-relationships)
5. [Class Hierarchies & Dependencies](#class-hierarchies--dependencies)
6. [Data Flow Examples](#data-flow-examples)
7. [Dependency Injection Container](#dependency-injection-container)

---

## ARCHITECTURE OVERVIEW

### Multi-Layer Architecture Pattern

```
┌─────────────────────────────────────────────────────────────┐
│                      API Layer                              │
│         Controllers, Routing, HTTP Handling                 │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ↓
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
│      Services, DTOs, Validation, Business Logic            │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ↓
┌─────────────────────────────────────────────────────────────┐
│                   Domain Layer                              │
│           Models, Entities, Business Rules                 │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ↓
┌─────────────────────────────────────────────────────────────┐
│                Infrastructure Layer                         │
│   Data Access, External Services, Authentication           │
└─────────────────────────────────────────────────────────────┘
```

### Design Principles

- **Unidirectional Dependency Flow:** Upper layers depend on lower layers only
- **Abstraction:** Each layer communicates through interfaces
- **Separation of Concerns:** Each layer has a single, well-defined responsibility
- **Testability:** Dependencies injected through interfaces for easy mocking
- **Flexibility:** Implementation details can change without affecting consumers

---

## LAYERED ARCHITECTURE

### Layer 1: API Layer (SkyRoute.Api)

**Responsibility:** HTTP request handling, routing, response formatting

**Components:**
- **Controllers** — REST endpoints
- **Middleware** — Cross-cutting concerns (error handling, logging)
- **Program.cs** — Application startup configuration

**What Lives Here:**
```
SkyRoute.Api/
├── Controllers/
│   ├── AirportsController.cs
│   ├── AuthController.cs
│   ├── BookingsController.cs
│   └── FlightsController.cs
├── Middleware/
│   └── ErrorHandlingMiddleware.cs
└── Program.cs
```

**Characteristics:**
- ✅ Thin controllers (5-20 lines of logic)
- ✅ All business logic delegated to services
- ✅ All validation delegated to validators
- ✅ HTTP concerns only

**Example: FlightsController**
```csharp
[ApiController]
[Route("api/flights")]
public class FlightsController : ControllerBase
{
    private readonly IFlightSearchService _flightSearchService;  // Depends on service interface
    private readonly IValidator<FlightSearchRequestDto> _searchValidator;

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] FlightSearchRequestDto request)
    {
        var validation = await _searchValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors });

        var result = await _flightSearchService.SearchFlightsAsync(request);  // Service call
        return Ok(result);
    }
}
```

---

### Layer 2: Application Layer (SkyRoute.Application)

**Responsibility:** Business logic orchestration, data transfer, validation, service implementation

**Components:**
- **Services** — Business logic implementation
- **DTOs** — Data contracts for API communication
- **Interfaces** — Service abstractions
- **Validators** — Input validation rules
- **Common** — Shared utilities

**What Lives Here:**
```
SkyRoute.Application/
├── Services/
│   ├── FlightSearchService.cs
│   ├── BookingService.cs
│   ├── AuthService.cs
│   ├── AirportService.cs
│   └── SeatService.cs
├── Interfaces/
│   ├── IFlightSearchService.cs
│   ├── IBookingService.cs
│   ├── IFlightRepository.cs
│   ├── IFlightPricingStrategy.cs
│   ├── ISeatService.cs
├── DTOs/
│   ├── Flight/
│   │   ├── FlightSearchRequestDto.cs
│   │   ├── FlightSearchResponseDto.cs
│   │   ├── FlightResultDto.cs
│   │   └── SeatAvailabilityDto.cs
│   ├── Booking/
│   ├── Auth/
│   └── Airport/
├── Validators/
│   ├── FlightSearchValidator.cs
│   ├── CreateBookingValidator.cs
│   └── RegisterRequestValidator.cs
└── Common/
    ├── AppException.cs
    ├── PricingResult.cs
    └── SeatConfiguration.cs
```

**Key Characteristics:**
- ✅ Zero dependencies on API layer
- ✅ Pure business logic — no HTTP concerns
- ✅ Depends on Domain and Infrastructure interfaces
- ✅ Highly testable (all dependencies are interfaces)

**Example: FlightSearchService**
```csharp
public class FlightSearchService : IFlightSearchService
{
    // Dependencies injected via constructor (all are interfaces)
    private readonly IFlightRepository _flightRepository;
    private readonly IAirportRepository _airportRepository;
    private readonly IEnumerable<IFlightPricingStrategy> _pricingStrategies;

    public async Task<FlightSearchResponseDto> SearchFlightsAsync(FlightSearchRequestDto request)
    {
        // 1. Validate airports exist
        var origin = await _airportRepository.GetByCodeAsync(request.OriginAirportCode)
            ?? throw new AppException("Origin airport not found", 404);

        var destination = await _airportRepository.GetByCodeAsync(request.DestinationAirportCode)
            ?? throw new AppException("Destination airport not found", 404);

        // 2. Get flights from database
        var flights = await _flightRepository.GetByRouteAndCabinAsync(
            origin.Id, destination.Id, request.CabinClass);

        // 3. Apply pricing strategies
        var results = new List<FlightResultDto>();
        foreach (var flight in flights)
        {
            var strategy = _pricingStrategies.FirstOrDefault(s => s.ProviderName == flight.Airline.Name);
            if (strategy == null) continue;

            var pricing = strategy.CalculatePrice(flight.BaseFare, request.NumberOfPassengers);

            results.Add(new FlightResultDto
            {
                AirlineName = flight.Airline.Name,
                FlightNumber = flight.FlightNumber,
                TotalPrice = pricing.TotalPrice,
                PricePerPassenger = pricing.PricePerPassenger,
                // ... other properties
            });
        }

        return new FlightSearchResponseDto { Results = results };
    }
}
```

**Key Point:** Service doesn't know or care which pricing strategies exist. It just loops through all registered `IFlightPricingStrategy` implementations. New providers are added without changing this code.

---

### Layer 3: Domain Layer (SkyRoute.Domain)

**Responsibility:** Core business entities, business rules, enums

**Components:**
- **Models** — Core business entities (Flight, User, Booking, etc.)
- **Enums** — Business domain constants

**What Lives Here:**
```
SkyRoute.Domain/
├── Models/
│   ├── Flight.cs
│   ├── User.cs
│   ├── Airport.cs
│   ├── Airline.cs
│   ├── Booking.cs
│   ├── PassengerDetail.cs
│   └── ...
└── Enums/
    ├── CabinClass.cs
    ├── DocumentType.cs
    ├── RouteType.cs
    └── ...
```

**Key Characteristics:**
- ✅ No dependencies on other layers
- ✅ Pure C# — no framework dependencies
- ✅ Encapsulates business meaning
- ✅ Can be used in any context (console app, API, service, etc.)

**Example: Flight Model**
```csharp
public class Flight
{
    public Guid Id { get; set; }
    public string FlightNumber { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public TimeSpan Duration { get; set; }
    public decimal BaseFare { get; set; }
    public CabinClass CabinClass { get; set; }

    // Navigation properties
    public Guid AirlineId { get; set; }
    public Airline Airline { get; set; }

    public Guid OriginAirportId { get; set; }
    public Airport OriginAirport { get; set; }

    public Guid DestinationAirportId { get; set; }
    public Airport DestinationAirport { get; set; }
}

public enum CabinClass
{
    Economy = 1,
    Business = 2,
    FirstClass = 3
}
```

---

### Layer 4: Infrastructure Layer (SkyRoute.Infrastructure)

**Responsibility:** Database access, external integrations, authentication, implementation details

**Components:**
- **Data** — Entity Framework DbContext, repository implementations
- **Pricing** — Concrete pricing strategy implementations
- **Authentication** — JWT token generation, user repository
- **Configurations** — Database configuration, dependency setup

**What Lives Here:**
```
SkyRoute.Infrastructure/
├── Data/
│   ├── SkyRouteDbContext.cs
│   ├── Repositories/
│   │   ├── FlightRepository.cs
│   │   ├── BookingRepository.cs
│   │   ├── AirportRepository.cs
│   │   ├── UserRepository.cs
│   │   └── ...
│   └── Migrations/
├── Pricing/
│   ├── GlobalAirPricingStrategy.cs
│   └── BudgetWingsPricingStrategy.cs
├── Authentication/
│   ├── JwtTokenProvider.cs
│   └── JwtSettings.cs
└── Configurations/
    └── ServiceRegistration.cs
```

**Key Characteristics:**
- ✅ Depends on Application and Domain layers
- ✅ Contains all database-specific code (EF Core)
- ✅ Implements interfaces defined in Application layer
- ✅ Safe to change without affecting business logic

**Example: GlobalAirPricingStrategy**
```csharp
public class GlobalAirPricingStrategy : IFlightPricingStrategy
{
    public string ProviderName => "GlobalAir";

    public PricingResult CalculatePrice(decimal baseFare, int numberOfPassengers)
    {
        var pricePerPassenger = Math.Round(baseFare * 1.15m, 2);  // 15% surcharge
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
```

**Example: FlightRepository**
```csharp
public class FlightRepository : IFlightRepository
{
    private readonly SkyRouteDbContext _context;

    public FlightRepository(SkyRouteDbContext context) => _context = context;

    public async Task<IEnumerable<Flight>> GetByRouteAndCabinAsync(Guid originId, Guid destId, CabinClass cabin)
    {
        return await _context.Flights
            .Where(f => f.OriginAirportId == originId && f.DestinationAirportId == destId && f.CabinClass == cabin)
            .Include(f => f.Airline)
            .Include(f => f.OriginAirport)
            .Include(f => f.DestinationAirport)
            .ToListAsync();
    }
}
```

---

## LAYER DETAILS & RESPONSIBILITIES

### API Layer Responsibilities

| Responsibility | Example | Anti-Pattern |
|---|---|---|
| Accept HTTP requests | `[HttpPost]`, `[FromBody]` | Putting business logic in controller |
| Validate request format | `[Required]`, `ProducesResponseType` | No validation at all |
| Call services | `await _service.DoSomething()` | Direct database queries |
| Return HTTP responses | `Ok()`, `BadRequest()`, `Unauthorized()` | Returning domain models directly |
| Set cookies/headers | `Response.Cookies.Append()` | Cookie logic in service layer |

### Application Layer Responsibilities

| Responsibility | Example | Anti-Pattern |
|---|---|---|
| Orchestrate business processes | Coordinate multiple repositories | Have any HTTP concerns |
| Apply business rules | Pricing calculation logic | Database queries directly |
| Validate data | FluentValidation rules | Database access in DTOs |
| Transform DTOs | Map Flight model to FlightResultDto | Exposing models via API |
| Handle errors gracefully | `AppException` with proper status codes | Throwing unhandled exceptions |

### Domain Layer Responsibilities

| Responsibility | Example | Anti-Pattern |
|---|---|---|
| Define entities | `Flight`, `User`, `Booking` classes | Framework dependencies in models |
| Define enums | `CabinClass { Economy, Business }` | Magic strings instead of enums |
| Express business meaning | Navigation properties, value types | Anemic models with only getters |
| Encapsulate rules | Validation in constructors | Validation only in services |

### Infrastructure Layer Responsibilities

| Responsibility | Example | Anti-Pattern |
|---|---|---|
| Database access | Entity Framework Core DbContext | Business logic in repositories |
| Query optimization | `.Include()` for eager loading | N+1 queries |
| Authentication implementation | JWT token generation | Hardcoded secrets |
| External service integration | Payment gateway, email service | Direct calls from services |
| Configuration | Connection strings, settings | Hardcoded values |

---

## INTERFACE RELATIONSHIPS

### Core Service Interfaces

```
IFlightSearchService
    ↓ implements
FlightSearchService
    ↓ depends on
    ├── IFlightRepository
    ├── IAirportRepository
    └── IEnumerable<IFlightPricingStrategy>

IBookingService
    ↓ implements
BookingService
    ↓ depends on
    ├── IBookingRepository
    ├── IFlightRepository
    └── IEnumerable<IFlightPricingStrategy>

IAirportService
    ↓ implements
AirportService
    ↓ depends on
    └── IAirportRepository

IAuthService
    ↓ implements
AuthService
    ↓ depends on
    ├── IUserRepository
    └── IJwtTokenProvider
```

### Repository Interfaces

```
IFlightRepository
├── GetByIdAsync(id)
├── GetByRouteAndCabinAsync(originId, destId, cabin)
└── ... (other queries)

IAirportRepository
├── GetAllAirportsAsync()
├── GetByCodeAsync(code)
└── ... (other queries)

IBookingRepository
├── CreateAsync(booking)
├── GetByReferenceAsync(referenceCode, userId)
├── GetOccupiedSeatsAsync(flightId, departureDate)

IUserRepository
├── GetByEmailAsync(email)
├── CreateAsync(user)
└── ... (other queries)
```

### Pricing Strategy Interface (The Extension Point!)

```
IFlightPricingStrategy
    ↑ implemented by
    ├── GlobalAirPricingStrategy
    └── BudgetWingsPricingStrategy
    
    (Can add more without changing anything!)
    ├── SkyEuropePricingStrategy (future)
    ├── AsiaFlyPricingStrategy (future)
    └── ... (unlimited providers)
```

---

## CLASS HIERARCHIES & DEPENDENCIES

### Dependency Graph: Flight Search Request

```
┌─────────────────────────────────────┐
│    HTTP Request (JSON)              │
└─────────────────┬───────────────────┘
                  │ deserialization
                  ↓
┌─────────────────────────────────────────────────────┐
│ FlightSearchRequestDto (API Layer)                  │
│ ├── OriginAirportCode: string                       │
│ ├── DestinationAirportCode: string                  │
│ ├── DepartureDate: DateOnly                         │
│ ├── NumberOfPassengers: int                         │
│ └── CabinClass: CabinClass enum                     │
└─────────────────┬───────────────────────────────────┘
                  │ validation
                  ↓
┌─────────────────────────────────────────────────────┐
│ FlightSearchValidator (Application Layer)           │
│ ├── Validate OriginAirportCode not empty            │
│ ├── Validate NumberOfPassengers 1-9                 │
│ └── Validate CabinClass valid enum                  │
└─────────────────┬───────────────────────────────────┘
                  │ valid ✓
                  ↓
┌─────────────────────────────────────────────────────┐
│ FlightsController.Search()                          │
│ └── calls ↓                                         │
│    IFlightSearchService.SearchFlightsAsync()        │
└─────────────────┬───────────────────────────────────┘
                  │
                  ↓
┌─────────────────────────────────────────────────────┐
│ FlightSearchService (Application Layer)             │
│                                                     │
│ Step 1: Validate airports exist                    │
│ ├── calls IAirportRepository.GetByCodeAsync()      │
│ └── throws AppException if not found               │
│                                                     │
│ Step 2: Fetch flights from database                │
│ ├── calls IFlightRepository.GetByRouteAndCabinAsync│
│ └── gets List<Flight>                              │
│                                                     │
│ Step 3: Apply pricing                              │
│ ├── loops through IEnumerable<IFlightPricingStrategy>
│ ├── calls CalculatePrice() for each strategy       │
│ └── gets PricingResult                             │
│                                                     │
│ Step 4: Transform to DTOs                          │
│ ├── creates FlightResultDto for each result        │
│ └── creates FlightSearchResponseDto                │
└─────────────────┬───────────────────────────────────┘
                  │
                  ↓
┌─────────────────────────────────────────────────────┐
│ FlightResultDto[] (Application Layer)               │
│ ├── AirlineName: string                            │
│ ├── FlightNumber: string                           │
│ ├── DepartureTime: DateTime                        │
│ ├── ArrivalTime: DateTime                          │
│ ├── TotalPrice: decimal                            │
│ ├── PricePerPassenger: decimal                     │
│ └── ... (other fields)                             │
└─────────────────┬───────────────────────────────────┘
                  │ serialization to JSON
                  ↓
┌─────────────────────────────────────────────────────┐
│ HTTP Response (200 OK)                              │
└─────────────────────────────────────────────────────┘
```

### Dependency Graph: Booking Creation

```
┌──────────────────────────────────────────────────────┐
│ HTTP Request + JWT Token (Authorization)            │
│ Body: CreateBookingRequestDto                       │
└─────────────────┬────────────────────────────────────┘
                  │
                  ↓
┌──────────────────────────────────────────────────────┐
│ BookingsController.CreateBooking()                   │
│ ├── Extract user ID from JWT claims                │
│ ├── Validate request with CreateBookingValidator   │
│ └── Call IBookingService.CreateBookingAsync()      │
└─────────────────┬────────────────────────────────────┘
                  │
                  ↓
┌──────────────────────────────────────────────────────┐
│ BookingService.CreateBookingAsync() (Application)   │
│                                                      │
│ Step 1: Fetch Flight                               │
│ ├── IFlightRepository.GetByIdAsync(flightId)       │
│ └── Validate flight exists & is available          │
│                                                      │
│ Step 2: Validate Passengers                         │
│ ├── Check each passenger document                  │
│ ├── Validate DocumentType matches RouteType        │
│ │  ├── Domestic: National ID required              │
│ │  └── International: Passport required            │
│                                                      │
│ Step 3: Calculate Price                             │
│ ├── Get pricing strategy for airline               │
│ ├── Call IFlightPricingStrategy.CalculatePrice()  │
│ └── Store final price in booking                   │
│                                                      │
│ Step 4: Create Booking Entity                       │
│ ├── Create Booking domain model                    │
│ ├── Add PassengerDetails                           │
│ ├── Generate BookingReferenceCode                  │
│ └── Call IBookingRepository.CreateAsync()          │
│                                                      │
│ Step 5: Map to Response                             │
│ └── Convert to BookingConfirmationDto              │
└─────────────────┬────────────────────────────────────┘
                  │
                  ↓
┌──────────────────────────────────────────────────────┐
│ BookingConfirmationDto (Application Layer)           │
│ ├── BookingReferenceCode: string (GUID)            │
│ ├── FlightDetails: FlightResultDto                 │
│ ├── PassengerCount: int                            │
│ ├── TotalPrice: decimal                            │
│ └── ConfirmationTime: DateTime                     │
└─────────────────┬────────────────────────────────────┘
                  │
                  ↓
┌──────────────────────────────────────────────────────┐
│ HTTP Response (201 Created)                          │
│ + Location header                                    │
└──────────────────────────────────────────────────────┘
```

---

## DATA FLOW EXAMPLES

### Example 1: Flight Search Flow

**Request:**
```json
POST /api/flights/search
{
    "originAirportCode": "NYC",
    "destinationAirportCode": "LAX",
    "departureDate": "2026-07-15",
    "numberOfPassengers": 2,
    "cabinClass": "Economy"
}
```

**Processing Chain:**

1. **API Layer (FlightsController)**
   ```
   Receives JSON → Deserializes to FlightSearchRequestDto
   ```

2. **Validation (Middleware/Validator)**
   ```
   Validates DTO properties:
   - OriginAirportCode not empty ✓
   - DestinationAirportCode not empty ✓
   - NumberOfPassengers in range 1-9 ✓
   - CabinClass valid enum ✓
   ```

3. **Service Layer (FlightSearchService)**
   ```
   a) Get origin airport
      → Call IAirportRepository.GetByCodeAsync("NYC")
      → Returns Airport { Id: guid1, Code: "NYC", ... }
   
   b) Get destination airport
      → Call IAirportRepository.GetByCodeAsync("LAX")
      → Returns Airport { Id: guid2, Code: "LAX", ... }
   
   c) Query flights
      → Call IFlightRepository.GetByRouteAndCabinAsync(guid1, guid2, Economy)
      → Returns List<Flight> with flights from NYC to LAX in Economy
   
   d) Calculate pricing for each flight
      For each flight:
         → Find matching IFlightPricingStrategy by Airline.Name
         → GlobalAirPricingStrategy: $100 base × 1.15 × 2 = $230 total
         → BudgetWingsPricingStrategy: ($100 - 10%) × 2 = $180 total
   
   e) Map to DTOs
      → Create FlightResultDto array with all pricing info
   ```

4. **Response**
   ```json
   HTTP 200 OK
   {
       "results": [
           {
               "id": "guid1",
               "airlineName": "GlobalAir",
               "flightNumber": "GA-101",
               "departureTime": "2026-07-15T14:30:00Z",
               "arrivalTime": "2026-07-15T20:15:00Z",
               "cabinClass": "Economy",
               "totalPrice": 230.00,
               "pricePerPassenger": 115.00,
               "pricingRule": "Base fare + 15% fuel surcharge"
           },
           {
               "id": "guid2",
               "airlineName": "BudgetWings",
               "flightNumber": "BW-205",
               "departureTime": "2026-07-15T15:00:00Z",
               "arrivalTime": "2026-07-15T20:45:00Z",
               "cabinClass": "Economy",
               "totalPrice": 180.00,
               "pricePerPassenger": 90.00,
               "pricingRule": "Base fare - 10% discount (minimum $29.99)"
           }
       ]
   }
   ```

---

### Example 2: Booking Creation Flow

**Request:**
```json
POST /api/bookings
Authorization: Bearer eyJhbGc...
{
    "flightId": "guid1",
    "passengers": [
        {
            "fullName": "John Doe",
            "documentType": "National ID",
            "documentNumber": "123456789",
            "email": "john@example.com"
        },
        {
            "fullName": "Jane Doe",
            "documentType": "National ID",
            "documentNumber": "987654321",
            "email": "jane@example.com"
        }
    ]
}
```

**Processing Chain:**

1. **Authentication Middleware**
   ```
   Extract JWT token → Validate signature → Extract user ID
   User ID: abc-def-ghi (GUID)
   ```

2. **API Layer (BookingsController)**
   ```
   Extract userId from claims → Pass to service
   Validate CreateBookingRequestDto
   ```

3. **Service Layer (BookingService)**
   ```
   a) Fetch Flight
      → IFlightRepository.GetByIdAsync("guid1")
      → Returns Flight object with airline info
   
   b) Determine Route Type
      → Check if domestic (same country) or international
      → Validate document types accordingly
   
   c) Validate Passengers
      → For domestic flight: National ID must be provided
      → For international flight: Passport must be provided
   
   d) Calculate Final Price
      → Get pricing strategy for airline (GlobalAir)
      → CalculatePrice(baseFare: 100, passengers: 2)
      → Returns PricingResult { TotalPrice: 230, PricePerPassenger: 115 }
   
   e) Create Booking Domain Entity
      booking = new Booking {
          Id = Guid.NewGuid(),
          BookingReferenceCode = Guid.NewGuid().ToString().Substring(0, 6),
          UserId = "abc-def-ghi",
          FlightId = "guid1",
          NumberOfPassengers = 2,
          TotalPrice = 230.00,
          Status = BookingStatus.Confirmed,
          CreatedAt = DateTime.UtcNow
      }
   
   f) Add Passenger Details
      booking.PassengerDetails = [
          new PassengerDetail { FullName: "John Doe", DocumentNumber: "123456789" },
          new PassengerDetail { FullName: "Jane Doe", DocumentNumber: "987654321" }
      ]
   
   g) Save to Database
      → IBookingRepository.CreateAsync(booking)
      → Booking persisted in database
   ```

4. **Response**
   ```json
   HTTP 201 Created
   Location: /api/bookings/A1B2C3
   {
       "bookingReferenceCode": "A1B2C3",
       "flightDetails": {
           "airlineName": "GlobalAir",
           "flightNumber": "GA-101",
           "departureTime": "2026-07-15T14:30:00Z",
           "arrivalTime": "2026-07-15T20:15:00Z"
       },
       "passengerCount": 2,
       "totalPrice": 230.00,
       "confirmationTime": "2026-06-19T10:30:00Z"
   }
   ```

---

## DEPENDENCY INJECTION CONTAINER

### How Dependencies Are Registered

**In Program.cs:**
```csharp
// Register services (Application Layer)
services.AddScoped<IFlightSearchService, FlightSearchService>();
services.AddScoped<IBookingService, BookingService>();
services.AddScoped<IAirportService, AirportService>();
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<ISeatService, SeatService>();

// Register repositories (Infrastructure Layer)
services.AddScoped<IFlightRepository, FlightRepository>();
services.AddScoped<IAirportRepository, AirportRepository>();
services.AddScoped<IBookingRepository, BookingRepository>();
services.AddScoped<IUserRepository, UserRepository>();

// Register pricing strategies (MULTIPLE implementations!)
services.AddScoped<IFlightPricingStrategy, GlobalAirPricingStrategy>();
services.AddScoped<IFlightPricingStrategy, BudgetWingsPricingStrategy>();
// When you add SkyEurope: just add this line
// services.AddScoped<IFlightPricingStrategy, SkyEuropePricingStrategy>();

// Register validators
services.AddScoped<IValidator<FlightSearchRequestDto>, FlightSearchValidator>();
services.AddScoped<IValidator<CreateBookingRequestDto>, CreateBookingValidator>();

// Register authentication
services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();

// Register database
services.AddDbContext<SkyRouteDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
```

### Dependency Resolution Example

**When FlightSearchService is requested:**

```
DI Container looks up IFlightSearchService
    → Found: FlightSearchService
    → FlightSearchService constructor needs:
        ├── IFlightRepository (registered → FlightRepository)
        ├── IAirportRepository (registered → AirportRepository)
        └── IEnumerable<IFlightPricingStrategy> (registered → [GlobalAirPricingStrategy, BudgetWingsPricingStrategy])

    → DI resolves each dependency:
        ├── IFlightRepository → Creates FlightRepository
        │   → Needs SkyRouteDbContext (registered)
        │   → Creates SkyRouteDbContext
        │
        ├── IAirportRepository → Creates AirportRepository
        │   → Needs SkyRouteDbContext (cached from above)
        │   → Reuses SkyRouteDbContext
        │
        └── IEnumerable<IFlightPricingStrategy> → Creates collection
            ├── GlobalAirPricingStrategy
            └── BudgetWingsPricingStrategy

    → Constructs FlightSearchService with all dependencies
    → Returns instance ready to use
```

---

## INTERFACE IMPLEMENTATION PATTERN

### The Pricing Strategy Pattern (Extensibility)

This pattern is used to enable new providers without modifying existing code.

**Interface (Application Layer):**
```csharp
// SkyRoute.Application/Interfaces/IFlightPricingStrategy.cs
public interface IFlightPricingStrategy
{
    string ProviderName { get; }  // e.g., "GlobalAir"
    PricingResult CalculatePrice(decimal baseFare, int numberOfPassengers);
}
```

**Implementation 1 (Infrastructure Layer):**
```csharp
// SkyRoute.Infrastructure/Pricing/GlobalAirPricingStrategy.cs
public class GlobalAirPricingStrategy : IFlightPricingStrategy
{
    public string ProviderName => "GlobalAir";
    
    public PricingResult CalculatePrice(decimal baseFare, int numberOfPassengers)
    {
        var pricePerPassenger = Math.Round(baseFare * 1.15m, 2);
        var totalPrice = Math.Round(pricePerPassenger * numberOfPassengers, 2);
        
        return new PricingResult
        {
            TotalPrice = totalPrice,
            PricePerPassenger = pricePerPassenger,
            PricingRule = "Base fare + 15% fuel surcharge"
        };
    }
}
```

**Implementation 2 (Infrastructure Layer):**
```csharp
// SkyRoute.Infrastructure/Pricing/BudgetWingsPricingStrategy.cs
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
            TotalPrice = totalPrice,
            PricePerPassenger = pricePerPassenger,
            PricingRule = $"Base fare - 10% discount (minimum ${MinimumPrice})"
        };
    }
}
```

**Consumer (Application Layer):**
```csharp
public class FlightSearchService : IFlightSearchService
{
    private readonly IEnumerable<IFlightPricingStrategy> _pricingStrategies;

    public FlightSearchService(IEnumerable<IFlightPricingStrategy> pricingStrategies)
    {
        _pricingStrategies = pricingStrategies;
    }

    public async Task<FlightSearchResponseDto> SearchFlightsAsync(FlightSearchRequestDto request)
    {
        var results = new List<FlightResultDto>();
        
        // Loop through ALL registered pricing strategies
        foreach (var flight in flights)
        {
            var strategy = _pricingStrategies.FirstOrDefault(s => s.ProviderName == flight.Airline.Name);
            if (strategy == null) continue;
            
            var pricing = strategy.CalculatePrice(flight.BaseFare, request.NumberOfPassengers);
            
            results.Add(new FlightResultDto
            {
                AirlineName = flight.Airline.Name,
                TotalPrice = pricing.TotalPrice,
                PricePerPassenger = pricing.PricePerPassenger,
                // ... other properties
            });
        }
        
        return new FlightSearchResponseDto { Results = results };
    }
}
```

**To Add a New Provider (SkyEurope):**

1. Create new class in Infrastructure:
```csharp
public class SkyEuropePricingStrategy : IFlightPricingStrategy
{
    public string ProviderName => "SkyEurope";
    
    public PricingResult CalculatePrice(decimal baseFare, int numberOfPassengers)
    {
        // SkyEurope pricing logic
    }
}
```

2. Register in DI (one line):
```csharp
services.AddScoped<IFlightPricingStrategy, SkyEuropePricingStrategy>();
```

3. Done! ✓ No changes to FlightSearchService or any other code!

---

## SUMMARY: Layered Architecture Benefits

| Benefit | How Achieved |
|---------|-------------|
| **Testability** | All dependencies are interfaces; easy to mock |
| **Maintainability** | Clear separation of concerns; each layer has one job |
| **Extensibility** | New providers added without touching existing code (Open/Closed Principle) |
| **Reusability** | Services can be reused across different contexts |
| **Independent Development** | Teams can work on different layers independently |
| **Framework Agnostic** | Domain layer doesn't depend on any framework |
| **Easy to Understand** | Clear contracts defined by interfaces |
| **Flexible Deployment** | Can be deployed as monolith, microservices, serverless |

---

**Document Version:** 1.0  
**Last Updated:** June 19, 2026  
**Audience:** Developers, Architects  
**Next Update:** After adding new provider integration
