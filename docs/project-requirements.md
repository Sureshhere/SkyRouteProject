# SkyRoute Travel Platform - Project Requirements

## 1. Overview & Context

SkyRoute is a travel aggregator platform that allows users to search, compare, and book flights. This project focuses on building the **Flight Search & Booking module** — a core feature of the platform.

The system must be well-architected and maintainable to support future growth and extensibility.

### Technology Stack
- **Frontend:** Angular
- **Backend:** .NET

---

## 2. Business Context

### Airline Provider Integration
SkyRoute aggregates flight data from multiple airline providers. Each provider has its own API and pricing model. For this implementation, you must integrate with two mock providers:

#### Provider 1: GlobalAir
- **Pricing Rule:** Base fare + 15% fuel surcharge
- **Rounding:** Always round the final price to 2 decimal places

#### Provider 2: BudgetWings
- **Pricing Rule:** Base fare − 10% promotional discount
- **Minimum Price:** $29.99 (discounts cannot reduce the price below this)
- **Discount Scope:** Discount is applied to base fare only

### Future Extensibility
The platform expects to onboard additional airline providers in the future. The architecture must support easy integration of new providers.

---

## 3. Functional Requirements

### 3.1 Flight Search Form

**Capture the following information:**
- **Origin Airport** (dropdown)
- **Destination Airport** (dropdown)
- **Departure Date** (date picker)
- **Number of Passengers** (1 to 9)
- **Cabin Class** (Economy, Business, or First Class)

**Airport Database:**
- Hardcode at least 6 airports
- Must span at least 2 countries
- Include both domestic and international routes

**Search Submission:**
- Frontend calls the backend API
- Display results as a list or table format

### 3.2 Flight Results Display

**Result Fields to Show:**
- Airline provider name
- Flight number
- Departure time
- Arrival time
- Duration
- Cabin class
- **Price (total for all passengers)**

**Pricing Display Requirements:**
The price shown must make a clear distinction between:
- **Total Price:** For all passengers combined (e.g., USD 320.00)
- **Per-Passenger Price:** Individual cost (e.g., USD 160.00 per person)

Display format example: `USD 320.00 total / USD 160.00 per person`

### 3.3 Flight Results & Sorting

**Sorting Capabilities:**
- Sort by **Price** (low to high and high to low)
- Sort by **Duration** (shortest first)
- Sort by **Departure Time**

**Important:** Sorting must happen on the frontend — no additional API call should be made when changing sort order.

**User Experience:**
- Show a **loading indicator** while the search is in progress
- Show a **clear empty state** if no flights match the search criteria

### 3.4 Booking Flow

#### Booking Screen Content:
1. **Flight Summary**
   - Route (origin → destination)
   - Airline provider
   - Departure and arrival times
   - Cabin class

2. **Price Breakdown**
   - Per-passenger price
   - Number of passengers
   - Total price

3. **Passenger Details Form**
   - Full name (for each passenger or consolidated)
   - Email address
   - Document number field (dynamic label)

#### Document Type Requirement (Important)

The document field label and validation **must change based on the selected route:**

| Route Type | Domestic | International |
|-----------|----------|-----------------|
| **Document Field Label** | National ID | Passport Number |
| **Validation Rule** | National ID format | Passport format |

For **international flights** (origin and destination in different countries):
- Label: "Passport Number"
- Apply passport validation rules

For **domestic flights** (origin and destination in the same country):
- Label: "National ID"
- Apply national ID validation rules

#### Booking Confirmation
- Submit passenger and flight details to the backend
- Return a **booking reference code** to the user
- Display confirmation with the reference code

### 3.5 Backend API

**Design and implement a backend API that supports:**
1. Flight search and filtering
2. Booking submission and confirmation
3. Provider integration (mock GlobalAir and BudgetWings)

**API Requirements:**
- Clear endpoint structure
- Proper contract definitions
- Error handling
- Validation

---

## 4. Technical Considerations

### Architecture & Design
- Must support easy addition of new airline providers
- Provider pricing logic should be cleanly separated and extendable
- Frontend should not know about provider-specific rules
- Proper separation of concerns between UI, business logic, and data access

### Mocking External Providers
- GlobalAir and BudgetWings APIs should be mocked in the backend
- Mocks should return realistic flight data sets for any given search query
- Mocks should apply their respective pricing rules correctly

### Data Validation
- Airport data validation (at least 6 airports across 2+ countries)
- Passenger count validation (1-9)
- Date validation (departure date should be in the future)
- Document number validation (different rules for domestic vs. international)
- Email validation

### Performance & UX
- Frontend-side sorting for instant user feedback (no API calls)
- Loading indicators for async operations
- Clear error messages and empty states

---

## 5. Deliverables

### Required Outputs:
1. **Working Application**
   - Frontend (Angular) running locally
   - Backend (.NET) API running locally
   - Complete flight search and booking flow functional

2. **Source Code Repository**
   - Well-organized directory structure
   - Clean, maintainable code
   - Meaningful commit history

3. **Documentation (README)**
   - Setup and run instructions (step-by-step)
   - Architecture decisions and design rationale
   - Trade-offs and any known limitations

**Note:** Local execution is sufficient — no cloud deployment required.

---

## 6. Project Status & Next Steps

- [ ] Backend API structure and endpoint design
- [ ] Mock provider implementations (GlobalAir, BudgetWings)
- [ ] Flight search endpoint
- [ ] Booking endpoint
- [ ] Angular frontend setup
- [ ] Search form component
- [ ] Results display component
- [ ] Booking flow component
- [ ] Frontend-client service for API communication
- [ ] README documentation
- [ ] Testing and validation
- [ ] Git repository and submission
