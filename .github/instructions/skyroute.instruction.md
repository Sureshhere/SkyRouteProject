# SkyRoute Travel Platform - Development Instructions

**Project:** SkyRoute Flight Search & Booking Platform  
**Tech Stack:** Angular 21 (Frontend) | ASP.NET Core Web API (Backend) | SQL Server (Database)  
**Architecture:** Service-Based with thin controllers and provider pattern for extensibility

---

## Core Principles

These principles guide ALL development decisions:

1. **Implement Only What's Required** - Follow the requirements document strictly. Avoid unnecessary enterprise complexity or speculative features.
2. **Prefer Simplicity** - Choose the simplest solution that solves the problem. Introduce complexity only when justified.
3. **Explain Before Coding** - Articulate design decisions and trade-offs before implementing.
4. **Keep Code Maintainable & Testable** - Write code that is easy to understand, modify, and test.
5. **Service-Based Architecture** - Separate concerns: Services contain business logic, Controllers orchestrate, DTOs define contracts.
6. **Apply SOLID Principles Where Practical** - Use Open/Closed, Dependency Injection, and Interface Segregation where they add value.
7. **Use Design Patterns Judiciously** - Apply patterns (Provider Pattern, Repository, etc.) only when they solve a real problem.

---

## Backend Development (.NET Core)

### API Architecture

- **RESTful Design**: Use standard HTTP methods and proper status codes (200, 201, 400, 404, 500, etc.)
- **DTOs for Contracts**: Never expose EF Core models directly; always use Data Transfer Objects for API requests/responses.
- **Async/Await Mandatory**: Use async methods throughout. Never use `.Result` or `.Wait()`.
- **Thin Controllers**: Controllers should orchestrate service calls, not contain business logic.

### Business Logic & Services

- **Service Layer Pattern**: Core business logic lives in services (SearchService, BookingService, PricingService, etc.).
- **Provider Integration Pattern**: Use `IFlightProvider` interface for airline integrations.
  - Enables adding new providers without modifying existing code.
  - Each provider (GlobalAir, BudgetWings) implements the interface.
- **Pricing Service**: Encapsulates all provider-specific pricing rules:
  - **GlobalAir**: Base fare + 15% fuel surcharge (round to 2 decimals)
  - **BudgetWings**: Base fare − 10% discount, minimum $29.99 (discount applies to base only)
  - Service calculates final prices; never expose raw pricing logic to controllers.

### Data Access

- **No N+1 Queries**: Use `.Include()` in EF Core to avoid multiple queries for related data.
- **Async Data Operations**: All database calls must be async.
- **Mock Providers for Development**: Hardcode mock data for GlobalAir and BudgetWings providers during development.

### Validation & Error Handling

- **Input Validation**: Validate at controller level (e.g., passenger count 1-9, future dates, valid airports).
- **Business Rule Validation**: Validate in services (e.g., minimum price rules, provider availability).
- **Consistent Error Responses**: Return meaningful error messages with appropriate HTTP status codes.
- **Passenger Count**: 1-9 passengers
- **Dates**: Departure date must be in the future
- **Document Validation**: Different rules for domestic vs. international (see Frontend section)

---

## Frontend Development (Angular 21)

### Component Architecture

- **Reactive Forms**: Use reactive forms with dynamic validation for all forms (search, booking, passenger details).
- **Service Layer**: Angular services handle HTTP communication and business logic.
- **Smart & Dumb Components**: Smart components manage state/logic; dumb components receive inputs and emit events.

### Flight Search Form

- **Capture Fields**:
  - Origin Airport (dropdown)
  - Destination Airport (dropdown)
  - Departure Date (date picker)
  - Number of Passengers (1-9, numeric input)
  - Cabin Class (Economy, Business, First Class - dropdown/radio)
- **Airport Data**: Hardcode at least 6 airports spanning 2+ countries (domestic and international routes).
- **Form Validation**: Validate all fields before submission; disable submit button if invalid.

### Flight Results Display

**Show the following fields for each result:**
- Airline provider name
- Flight number
- Departure time
- Arrival time
- Duration
- Cabin class
- **Price Display (CRITICAL)**:
  - Format: `USD 320.00 total / USD 160.00 per person`
  - Make distinction between total and per-passenger price VERY clear
  - Calculate: Per-person price = Total price ÷ Number of passengers

**User Experience:**
- Show loading indicator while search is in progress
- Show empty state message if no results match the search criteria
- Make results easy to scan (use table or card layout)

### Flight Results Sorting

- **Sorting Options**: Price (asc/desc), Duration (shortest first), Departure Time (earliest/latest)
- **Frontend-Side Only**: Sorting must happen on the frontend with NO additional API calls.
- **Sort State**: Maintain sort preference as user switches between sorts.

### Booking Flow

#### Booking Screen Content:

1. **Flight Summary Section**:
   - Route: Origin → Destination
   - Airline provider name
   - Departure and arrival times
   - Cabin class
   - Visual indication of booking context

2. **Price Breakdown Section**:
   - Per-passenger price
   - Number of passengers
   - Total price (clearly highlighted)

3. **Passenger Details Form** (Dynamic field labels):
   - Full name (for each passenger or consolidated form)
   - Email address
   - **Document Field (IMPORTANT - See Below)**

#### Document Field - Route-Based Dynamics

The document field label and validation **MUST change based on route type**:

| Scenario | Label | Validation Rule |
|----------|-------|-----------------|
| **International Flight** (different countries) | "Passport Number" | Passport format validation |
| **Domestic Flight** (same country) | "National ID" | National ID format validation |

**Implementation:**
- Determine route type: if origin country ≠ destination country → International, else → Domestic
- Update form label dynamically based on route type
- Apply appropriate validation rules to the document field
- Store document type with booking for backend processing

#### Booking Submission

- Submit passenger details + flight selection to backend
- Receive booking reference code from backend
- Display confirmation screen with:
  - Booking reference code (prominent display)
  - Flight and passenger details summary
  - Confirmation message

---

## API Endpoints (Backend)

### Proposed Endpoint Structure

**Flight Search:**
```
POST /api/flights/search
Request: { origin, destination, departureDate, passengerCount, cabinClass }
Response: [ { flightNumber, provider, departureTime, arrivalTime, duration, cabinClass, totalPrice, perPersonPrice } ]
```

**Booking:**
```
POST /api/bookings
Request: { flightDetails, passengerDetails[], documentType, documentNumber }
Response: { bookingReferenceCode, confirmationDetails }
```

---

## Provider Implementation Rules

### GlobalAir Provider
- **Pricing Formula**: `finalPrice = baseFare * 1.15` (rounded to 2 decimals)
- **Mock Data**: Return realistic flight data for any search query
- **No Minimum Price**: Apply surcharge regardless of base fare

### BudgetWings Provider
- **Pricing Formula**: `discountedPrice = max(baseFare * 0.90, 29.99)`
- **Minimum Price**: Discount cannot reduce price below $29.99
- **Mock Data**: Return realistic flight data for any search query

### Adding New Providers

To integrate a new provider:
1. Create new class implementing `IFlightProvider` interface
2. Add provider-specific pricing logic to `PricingService`
3. Register provider in dependency injection container
4. No changes needed to existing provider implementations

---

## Code Quality & Maintainability

### Naming & Organization
- **Controllers**: `[Resource]Controller` (e.g., `FlightsController`, `BookingsController`)
- **Services**: `[Resource]Service` (e.g., `FlightSearchService`, `BookingService`)
- **DTOs**: `[Resource]Dto` (e.g., `FlightSearchRequestDto`, `BookingResponseDto`)
- **Interfaces**: `I[Name]` (e.g., `IFlightProvider`, `IPricingService`)

### Code Style
- Use `async/await` consistently; no `.Result` or `.Wait()`
- Keep methods focused and under 15 lines where practical
- Use meaningful variable names; avoid abbreviations
- Add comments for non-obvious business logic

### Testing Strategy
- Write unit tests for service layer (business logic)
- Test provider pricing calculations with edge cases
- Test form validation in Angular components
- Test API error handling and status codes

---

## When Generating Code

1. **Follow Existing Project Structure**: Respect established file organization and naming conventions.
2. **Do Not Introduce Unnecessary Patterns**: Use patterns only when they solve a specific problem.
3. **Keep Controllers Thin**: Move logic to services.
4. **Use Async Methods**: Always async, no blocking calls.
5. **Use DTOs for API Contracts**: Separate API models from business models.
6. **Explain Trade-Offs**: If choosing simplicity over a "standard" pattern, explain why.

---

## Common Gotchas (Avoid These)

1. **Do NOT expose EF Core models in API responses** - Use DTOs only.
2. **Do NOT use `.Result` or `.Wait()`** - Always use `await` for async operations.
3. **Do NOT calculate discounts before applying minimum price** - BudgetWings: apply discount, THEN enforce minimum.
4. **Do NOT call API again when sorting results** - Sorting happens on the frontend with existing data.
5. **Do NOT forget to display BOTH total and per-passenger prices** - This is a key UX requirement.
6. **Do NOT hardcode pricing logic in controllers** - Business logic belongs in services.
7. **Do NOT ignore document field validation differences** - Domestic vs. International validation rules are critical.
8. **Do NOT create N+1 query scenarios** - Use `.Include()` for related entities.

---

## Documentation Requirements

- **README.md** in project root with:
  - Step-by-step setup and run instructions
  - Architecture overview and design rationale
  - Known limitations and trade-offs
  - API endpoint summary
  - Future extensibility notes (how to add new providers)

---

## Project Checklist

Use this to track implementation progress:

- [ ] Backend: API structure and endpoint design
- [ ] Backend: Mock provider implementations (GlobalAir, BudgetWings)
- [ ] Backend: Flight search endpoint and service layer
- [ ] Backend: Booking endpoint and reference code generation
- [ ] Backend: Price calculation and validation
- [ ] Frontend: Angular project setup
- [ ] Frontend: Search form component with validation
- [ ] Frontend: Results display with price formatting
- [ ] Frontend: Frontend-side sorting implementation
- [ ] Frontend: Booking flow component
- [ ] Frontend: Dynamic document field (domestic/international)
- [ ] Frontend: HTTP client service for API communication
- [ ] Integration: End-to-end search and booking flow
- [ ] Documentation: README with setup and architecture notes
- [ ] Testing: Unit tests for core services and business logic
