# SkyRoute Travel Platform — Complete Project Overview

> A flight search and booking platform built end-to-end using **Angular 21** + **ASP.NET Core (.NET 8)** + **SQL Server**, with a structured AI-assisted development workflow powered by **GitHub Copilot Agents and Skills**.

---

## Table of Contents

1. [What Is SkyRoute?](#1-what-is-skyroute)
2. [Application Features](#2-application-features)
3. [Tech Stack at a Glance](#3-tech-stack-at-a-glance)
4. [Project Structure](#4-project-structure)
5. [Frontend — Angular 21](#5-frontend--angular-21)
6. [Backend — ASP.NET Core (.NET 8)](#6-backend--aspnet-core-net-8)
7. [Database — SQL Server + EF Core](#7-database--sql-server--ef-core)
8. [Authentication — HttpOnly Cookie JWT](#8-authentication--httponly-cookie-jwt)
9. [CI/CD — GitHub Actions](#9-cicd--github-actions)
10. [Testing Strategy](#10-testing-strategy)
11. [How GitHub Copilot Was Used](#11-how-github-copilot-was-used)
12. [Copilot Agents](#12-copilot-agents)
13. [Copilot Skills](#13-copilot-skills)
14. [Copilot Instructions File](#14-copilot-instructions-file)
15. [Key Design Decisions](#15-key-design-decisions)
16. [Development Philosophy](#16-development-philosophy)

---

## 1. What Is SkyRoute?

**SkyRoute** is a flight aggregation and booking platform. It aggregates flight data from multiple airline providers, lets users search for available flights, view results, and complete a full booking workflow with passenger details.

The platform currently supports two airline providers:

| Provider | Pricing Model |
|----------|--------------|
| **GlobalAir** | Base fare + 15% surcharge |
| **BudgetWings** | Base fare − 10% discount, minimum $29.99 |

The architecture is designed so that future airline providers can be added with zero changes to existing code — just add a new pricing strategy and register it.

---

## 2. Application Features

| Feature | Description |
|---------|-------------|
| **User Registration** | Register with name, email, and password (BCrypt hashed) |
| **User Login / Logout** | JWT stored as HttpOnly cookie — never exposed to JavaScript |
| **Flight Search** | Search by origin, destination, date, passengers, and cabin class |
| **Flight Results** | Display flights from all providers with client-side sorting |
| **Booking** | Book a flight with full passenger details |
| **Booking Confirmation** | View booking reference code and full booking summary |
| **Document Validation** | Dynamic: National ID for domestic flights, Passport for international |
| **Price Breakdown** | Total price and per-passenger price clearly displayed |

---

## 3. Tech Stack at a Glance

```
┌──────────────────────────────────────────────────────────────────┐
│                        FRONTEND                                  │
│  Angular 21 · Standalone Components · Signals · Reactive Forms  │
│  TypeScript 5.9 · RxJS 7.8 · Karma + Jasmine (tests)           │
├──────────────────────────────────────────────────────────────────┤
│                        BACKEND                                   │
│  ASP.NET Core Web API · .NET 8 · C#                             │
│  FluentValidation · BCrypt · JWT Bearer (HttpOnly cookie)        │
│  Swagger / OpenAPI · xUnit + Moq + FluentAssertions (tests)     │
├──────────────────────────────────────────────────────────────────┤
│                        DATABASE                                  │
│  SQL Server (LocalDB in dev) · Entity Framework Core 8          │
│  Code-First Migrations · Auto-seeded flight data                │
├──────────────────────────────────────────────────────────────────┤
│                        CI / CD                                   │
│  GitHub Actions · Auto PR · Auto Merge · Copilot Review         │
└──────────────────────────────────────────────────────────────────┘
```

---

## 4. Project Structure

```
SkyRouteProject/
├── .github/
│   ├── copilot-instructions.md    # Repository-level Copilot system prompt
│   ├── instructions/
│   │   └── skyroute.instruction.md
│   ├── agents/                    # 6 Copilot Coding Agents
│   │   ├── architect.agent.md
│   │   ├── backend.agent.md
│   │   ├── frontend.agent.md
│   │   ├── tester.agent.md
│   │   ├── reviewer.agent.md
│   │   └── doc-sync.agent.md
│   ├── skills/                    # 5 Copilot Skills (reference libraries)
│   │   ├── dotnet-skill.md
│   │   ├── angular-skill.md
│   │   ├── architecture-skill.md
│   │   ├── review-skill.md
│   │   └── testing-skill.md
│   └── workflows/                 # GitHub Actions CI/CD
│       ├── ci.yml
│       ├── auto-pr.yml
│       ├── auto-merge.yml
│       ├── copilot-review.yml
│       └── release.yml
├── Backend/
│   ├── SkyRoute.Api/              # Controllers, Middleware, Program.cs
│   ├── SkyRoute.Application/      # Services, DTOs, Validators, Interfaces
│   ├── SkyRoute.Domain/           # Domain models, Enums (zero dependencies)
│   ├── SkyRoute.Infrastructure/   # EF Core, Repositories, Auth, Pricing
│   └── SkyRoute.Tests/            # xUnit test project
├── Frontend/
│   └── src/app/
│       ├── auth/                  # Login + Register
│       ├── flights/               # Search + Results
│       ├── booking/               # Booking + Confirmation
│       ├── shared/                # Empty / Error / Loading states
│       ├── guards/                # Auth guard
│       ├── interceptors/          # HTTP interceptor (401 handling)
│       ├── services/              # Auth, Flight, Booking services
│       └── models/                # Shared TypeScript interfaces
└── docs/                          # Architecture, requirements, this file
```

---

## 5. Frontend — Angular 21

### Architecture

The frontend is a **Single Page Application** built with Angular 21 using all modern patterns:
- **Standalone Components** — no NgModules
- **Angular Signals** — reactive state without NgRx or extra libraries
- **Reactive Forms** — strongly typed forms with custom validators

### Pages and Routing

| Route | Component | Auth Required |
|-------|-----------|--------------|
| `/` | Redirects to `/flights` | No |
| `/register` | `RegisterComponent` | No |
| `/login` | `LoginComponent` | No |
| `/flights` | `FlightSearchComponent` | No |
| `/results` | `FlightResultsComponent` | No |
| `/booking` | `BookingComponent` | **Yes** |
| `/confirmation` | `ConfirmationComponent` | **Yes** |

### State Management

No external state library (NgRx, Akita, etc.) — uses Angular's built-in **Signals**:

```typescript
// Example from AuthService
isAuthenticated = signal(false);
userEmail = computed(() => this.isAuthenticated() ? localStorage.getItem('email') : null);
```

### Key Services

| Service | Responsibility |
|---------|---------------|
| `AuthService` | Register, login, logout; auth state via signals |
| `FlightService` | Search flights, fetch airports, `getAvailableSeats`, client-side sort, format helpers |
| `BookingService` | Create/fetch/cancel booking, document validation, domestic check |

### Form Validation

All forms use Reactive Forms. Custom validators include:

- **`sameAirportValidator`** (cross-field): origin and destination cannot be the same
- **`pastDateValidator`**: departure date must be today or in the future
- **Dynamic document validator**: switches regex between National ID and Passport based on whether the route is domestic or international

### UX States

Every feature always renders one of three states:
- **Loading**: spinner shown while API call is in progress
- **Error**: user-friendly message, no internal details exposed
- **Empty**: guidance shown when no results are returned

---

## 6. Backend — ASP.NET Core (.NET 8)

### Layered Clean Architecture

```
SkyRoute.Api
    ↓
SkyRoute.Application   (interfaces, services, DTOs, validators)
    ↓
SkyRoute.Infrastructure   (EF Core, pricing strategies, JWT provider)
    ↓
SkyRoute.Domain   (models, enums — no external dependencies)
```

Dependencies always flow **inward**. The Domain layer has zero dependencies on any other layer or NuGet package.

### API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/airports` | None | Returns all airports |
| `POST` | `/api/flights/search` | None | Search flights (FluentValidation) |
| `GET` | `/api/flights/{flightId}/seats` | Bearer | Returns available seats for a flight on a date |
| `POST` | `/api/bookings` | Bearer | Create a booking |
| `GET` | `/api/bookings/{referenceCode}` | Bearer | Fetch booking by reference |
| `POST` | `/api/auth/register` | None | Register new user |
| `POST` | `/api/auth/login` | None | Login, sets HttpOnly cookie |
| `POST` | `/api/auth/logout` | None | Clears auth cookie |

### Thin Controllers, Fat Services

Controllers only: receive request → validate → call service → return response.

Business logic lives entirely in services:
- `AuthService` — registration, login, BCrypt hashing (workFactor 12), JWT generation
- `FlightSearchService` — airport lookup, flight query, pricing strategy resolution
- `BookingService` — flight validation, domestic/international detection, document validation, booking creation with reference code (`SK{yyyyMMdd}{6-hex}`)
- `SeatService` — queries occupied seats per flight/date, computes available seats from flight capacity

### Pricing — Strategy Pattern

Adding a new airline provider requires only two steps:
1. Create a new class implementing `IFlightPricingStrategy`
2. Register it with Dependency Injection

Existing code is **untouched** — Open/Closed Principle in action.

```csharp
public interface IFlightPricingStrategy
{
    string ProviderName { get; }
    decimal CalculatePrice(decimal baseFare);
}

// GlobalAir: baseFare * 1.15
// BudgetWings: max(baseFare * 0.90, 29.99)
```

### Input Validation — FluentValidation

All inputs are validated server-side via FluentValidation:
- `RegisterRequestValidator` — email format, password length (8–100), name required
- `FlightSearchValidator` — codes required and not equal, date in future, passengers 1–9
- `CreateBookingValidator` — flight ID, departure date, at least 1 passenger
- `DocumentNumberValidator` — National ID (8–12 alphanumeric) vs Passport (6–9 alphanumeric)

### Error Handling

`ErrorHandlingMiddleware` catches all exceptions globally:
- Known `AppException` → mapped to appropriate HTTP status (404, 409, etc.)
- Unhandled exception → 500 with a generic `{ "error": "..." }` JSON response
- Internal implementation details are never exposed to the client

---

## 7. Database — SQL Server + EF Core

### Configuration

- **Engine**: SQL Server
- **Development**: `(localdb)\MSSQLLocalDB` (SQL Server LocalDB)
- **ORM**: Entity Framework Core 8, Code-First
- **Migrations**: Auto-applied on startup via `db.Database.Migrate()`

### Schema

```
Users
├── Id (GUID PK)
├── Email (unique, max 256)
├── PasswordHash
├── FullName (max 200)
├── CreatedAt
└── IsActive

Airlines
├── Id (GUID PK)
├── Name (unique, max 100)
├── Code (max 10)
└── IsActive

Airports
├── Id (GUID PK)
├── Code (unique, max 10)
├── Name, City, Country
├── CountryCode (max 5)
└── IsActive

Flights
├── Id (GUID PK)
├── AirlineId (FK → Airlines)
├── FlightNumber
├── OriginAirportId (FK → Airports, Restrict)
├── DestinationAirportId (FK → Airports, Restrict)
├── DepartureTime, ArrivalTime, DurationMinutes
├── CabinClass (Economy | Business | FirstClass)
├── BaseFare (decimal 18,2)
└── IsActive

Bookings
├── Id (GUID PK)
├── BookingReferenceCode (unique, max 20)
├── UserId (FK → Users, cascade delete)
├── FlightId (FK → Flights, restrict)
├── DepartureDate
├── NumberOfPassengers
├── TotalPrice / PricePerPassenger (decimal 18,2)
├── Status (Confirmed | Cancelled)
├── CreatedAt, UpdatedAt
└── PassengerDetails (1:N)

PassengerDetails
├── Id (GUID PK)
├── BookingId (FK → Bookings, cascade)
├── FullName, Email
├── DocumentType (NationalId | PassportNumber)
├── DocumentNumber
├── SeatNumber (NVARCHAR(5))
├── PassengerIndex
└── CreatedAt
```

### Seeded Data

Data is seeded through EF Core migrations so the app works immediately after setup:

| Category | Count | Details |
|----------|-------|---------|
| Airlines | 2 | GlobalAir (GA), BudgetWings (BW) |
| Airports | 6 | JFK, LAX, ORD (US), LHR (GB), BOM, DEL (IN) |
| Flights | **144** | 12 routes × 3 cabin classes × 4 time slots × 2 airlines |

The 12 routes include both domestic (e.g. JFK↔LAX within US, BOM↔DEL within IN) and international (e.g. JFK↔LHR, JFK↔BOM) to exercise the document validation logic.

---

## 8. Authentication — HttpOnly Cookie JWT

A deliberate security upgrade was made during development: JWT tokens were **migrated from localStorage to HttpOnly cookies**.

### Why HttpOnly Cookies?

| Risk | localStorage | HttpOnly Cookie |
|------|-------------|-----------------|
| XSS attack steals token | Vulnerable | **Protected** — JS cannot read it |
| CSRF attack | Not applicable | Mitigated with SameSite=Strict |

### How It Works

```
1. User logs in → POST /api/auth/login
2. Backend validates credentials
3. Backend generates JWT and sets it as:
   Set-Cookie: auth_token=<jwt>; HttpOnly; SameSite=Strict; Path=/
4. Frontend stores display state (email, name) in localStorage only
5. Every API call is made with `withCredentials: true`
6. Browser sends the cookie automatically — frontend never touches the token
7. Backend reads token from cookie via JwtBearerEvents.OnMessageReceived
```

### JWT Configuration

- **Issuer / Audience**: `SkyRoute`
- **Expiry**: 1 hour
- **ClockSkew**: `TimeSpan.Zero` (no tolerance on expiry)
- **Claims**: `sub` (userId GUID), `email`, `fullName`, `jti`, `iat`

---

## 9. CI/CD — GitHub Actions (Event Automation)

The entire development lifecycle is automated using **5 GitHub Actions workflows**. The key idea is: a developer only needs to push code — every other event (PR creation, CI run, code review trigger, merge, release) happens automatically based on GitHub events.

### How GitHub Events Are Automated — The Full Flow

```
Developer pushes to feature/xxx
           │
           ▼
  [auto-pr.yml fires]
  → Reads branch name → auto-generates PR title
  → Creates PR targeting `develop`
  → Requests reviewer (Sureshhere)
           │
           ▼
  [ci.yml fires on the new PR]
  → Restores NuGet packages
  → Builds solution (Release config)
  → Runs all xUnit tests with Cobertura coverage
  → Uploads test results + coverage as artifacts
  → Posts ✅ / ❌ comment on the PR with a link to the run
           │
           ▼
  [auto-merge.yml fires on the PR]
  → Polls the "Build & Test" check every 15 seconds
  → If CI passes → squash-merges into `develop` automatically
  → If CI fails → posts 🚫 blocking comment on the PR
           │
           ▼
  [copilot-review.yml fires on the PR]
  → Posts instructions for requesting a Copilot review
  → (Full automated Copilot review requires GitHub Enterprise plan)
           │
           ▼
  Someone merges develop → main
           │
           ▼
  [release.yml fires on push to main]
  → Builds and tests the release solution (.slnx)
  → Publishes a release summary with commit SHA and date
```

---

### Workflow 1 — `ci.yml` (Core CI Pipeline)

**Trigger**: Any PR targeting `develop` or `main`; also fires on direct push to `develop`

**What it does step by step**:
1. Checks out code on `ubuntu-latest`
2. Sets up .NET 8 SDK
3. Restores NuGet dependencies for `SkyRoute.sln`
4. Builds solution in Release configuration
5. Runs all xUnit tests with `--collect:"XPlat Code Coverage"` → generates Cobertura XML
6. Uploads `.trx` test results as a build artifact
7. Uploads `coverage.cobertura.xml` as a build artifact
8. If triggered by a PR → posts a comment on the PR:
   - `✅ CI Result: All checks passed — ready to merge.`
   - `❌ CI Result: Build or tests failed. Please review the logs.` + link to run

**Permissions used**: `contents: read`, `checks: write`, `pull-requests: write`

---

### Workflow 2 — `auto-pr.yml` (Automatic PR Creation)

**Trigger**: Push to any branch matching `feature/**`, `fix/**`, or `chore/**`

**What it does step by step**:
1. Reads the branch name from `context.ref`
2. Splits branch into type (`feature`, `fix`, `chore`) and name
3. Maps type to a prefix: `feature/` → `Feature:`, `fix/` → `Fix:`, `chore/` → `Chore:`
4. Converts hyphens to spaces to form a readable PR title
   - Example: `feature/httponly-cookie-auth` → `Feature: httponly cookie auth`
5. Checks if an open PR already exists for this branch (prevents duplicates)
6. Creates the PR targeting `develop` with an auto-generated checklist body
7. Requests `Sureshhere` as a reviewer

**Why this matters**: You never have to manually open a PR. Every feature branch automatically gets a PR the moment you push.

**Permissions used**: `contents: write`, `pull-requests: write`, `issues: write`

---

### Workflow 3 — `auto-merge.yml` (CI-Gated Auto Merge)

**Trigger**: PR opened, updated, or reopened targeting `develop` — **only for `feature/**` branches**

**What it does step by step**:
1. Uses `fountainhead/action-wait-for-check@v1.2.0` to poll the `Build & Test` check
   - Polls every **15 seconds**, times out after **600 seconds** (10 minutes)
2. **If CI passed** → calls GitHub API to squash-merge the PR:
   - Commit title: `Feature: branch name (#PR_NUMBER)`
   - Commit message: `Auto-merged by CI after all checks passed.`
3. **If CI failed** → posts a blocking comment on the PR:
   - `🚫 Auto-merge blocked — CI checks did not pass. Fix the failing checks and push again.`
   - Also fails the workflow step so the PR stays open

**Why squash merge?**: All commits from the feature branch are squashed into a single clean commit on `develop`, keeping history linear.

**Note**: `fix/**` and `chore/**` branches are intentionally excluded (`if: startsWith(github.head_ref, 'feature/')`) — these require manual review before merge.

---

### Workflow 4 — `copilot-review.yml` (Copilot Review Trigger)

**Trigger**: PR opened, updated, or reopened (any branch)

**What it does**:
- Prints instructions in the workflow log for how to manually request a GitHub Copilot review
- Posts step-by-step guidance: Open PR → Click ⚙️ next to Reviewers → Search "Copilot" → Select

**Why it exists**: Fully automated Copilot reviews require GitHub Copilot Enterprise. This workflow acts as a reminder and guide, ensuring every PR has the option to get an AI code review alongside the automated `@reviewer` agent.

---

### Workflow 5 — `release.yml` (Release to Main)

**Trigger**: Push to `main` branch

**What it does step by step**:
1. Checks out full git history (`fetch-depth: 0`)
2. Sets up .NET 8 SDK
3. Restores, builds, and runs tests on `SkyRoute.slnx` (the release solution file)
4. Creates a release summary log with the short commit SHA and date

**Why a separate solution file?**: `SkyRoute.slnx` is the newer solution format (XML-based). Running tests again on `main` is a final safety net — even if the PR merge introduced an unexpected issue, the release pipeline will catch it.

---

### Branch Protection Summary

| Branch | PR Required | CI Must Pass | Auto-Merge |
|--------|------------|-------------|-----------|
| `develop` | Yes (auto-created) | Yes | Yes (feature branches only) |
| `main` | Yes (manual) | Yes (release.yml reruns) | No |

### Permissions Model

All workflows use `secrets.GITHUB_TOKEN` — the built-in token GitHub provides automatically for each workflow run. No external secrets or PATs are required. Each workflow declares only the minimum permissions it needs.

---

## 10. Testing Strategy

### Backend Tests (xUnit)

| Test File | What Is Tested | Count |
|-----------|---------------|-------|
| `AuthServiceTests` | Register, login, BCrypt hashing, email normalization | 17 |
| `AuthControllerTests` | HTTP status codes, validation pipeline | 17 |
| `JwtBearerEventsTests` | Cookie vs Authorization header token extraction | 11 |
| `BookingControllerAuthTests` | `[Authorize]` enforcement, user ID extraction | 11 |
| `AuthValidationTests` | RegisterRequestValidator (all field rules) | 19 |
| `DocumentNumberValidatorTests` | National ID and passport format validation | — |
| `FlightSearchValidatorTests` | Airport codes, dates, passenger limits | 9 |
| `GlobalAirPricingStrategyTests` | 15% surcharge, decimal rounding | 5 |
| `BudgetWingsPricingStrategyTests` | 10% discount, minimum $29.99 floor | — |
| `BookingServiceTests` | Full booking flow, pricing, document logic | — |

**Stack**: xUnit · Moq · FluentAssertions · EF Core InMemory

### Frontend Tests (Karma + Jasmine)

- `auth.service.spec.ts` — auth state, login/logout flows
- `booking.service.spec.ts` — booking creation, document validation
- `auth.interceptor.spec.ts` — 401 interception and redirect
- `confirmation.component.spec.ts` — confirmation page rendering

**Coverage output**: `Frontend/coverage/skyroute/`

---

## 11. How GitHub Copilot Was Used

GitHub Copilot was used as a **structured development assistant** — not just for autocomplete, but as a team of specialized AI agents following project-specific instructions.

The workflow was:

```
Write requirements / architecture docs
         ↓
Configure Copilot via copilot-instructions.md
         ↓
Invoke specialized agents (@architect, @backend, @frontend, @tester, @reviewer)
         ↓
Each agent reads its role-specific skill files as reference
         ↓
doc-sync agent automatically keeps documentation in sync with code changes
         ↓
GitHub Actions CI validates every change automatically
```

This created a **consistent development discipline**: every feature was analyzed before being built, implemented by a role-specific agent that understood the architecture, tested by a dedicated testing agent, and reviewed by a reviewer agent acting as a Principal Engineer.

---

## 12. Copilot Agents

Six **GitHub Copilot Coding Agents** were defined in `.github/agents/`. Each is a markdown file with a `description` frontmatter (used for routing) and detailed role instructions.

### `@architect` — Architecture Agent
**Invoked as**: `@architect` in Copilot Chat  
**Purpose**: Turns requirements into implementation-ready designs. Does **not** write code — focuses on requirement analysis, API design, data model design, and tradeoff analysis.

**Workflow**:
1. Analyze requirements and constraints
2. Impact analysis (backend, frontend, database)
3. Architecture recommendation with reasoning
4. Provide API contracts, data model, component structure

### `@backend` — Backend Agent
**Invoked as**: `@backend` in Copilot Chat  
**Purpose**: Implements ASP.NET Core features following SkyRoute conventions.

**Responsibilities**: REST APIs, services, DTOs, FluentValidation, EF Core, error handling  
**Anti-patterns it avoids**: CQRS, MediatR, Repository Pattern, Unit of Work, Event Sourcing  
**Reference**: Consults `dotnet-skill.md` for patterns

### `@frontend` — Frontend Agent
**Invoked as**: `@frontend` in Copilot Chat  
**Purpose**: Implements Angular features using modern patterns.

**Responsibilities**: Standalone components, signals, reactive forms, routing, API integration  
**UX standard enforced**: Every feature must have loading, error, and empty states  
**Reference**: Consults `angular-skill.md` for patterns

### `@tester` — Testing Agent
**Invoked as**: `@tester` in Copilot Chat  
**Purpose**: Generates comprehensive tests with meaningful coverage.

**Test priorities** (in order):
1. Business rules (pricing, booking logic)
2. Pricing calculations (provider-specific formulas)
3. Validation logic (document requirements, passenger limits)
4. Booking flow (end-to-end)
5. Search behavior

**Reference**: Consults `testing-skill.md` for patterns

### `@reviewer` — Reviewer Agent
**Invoked as**: `@reviewer` in Copilot Chat  
**Purpose**: Acts as a Principal Engineer reviewing completed work.

**Review dimensions**: Requirements, Architecture, Implementation (backend + frontend), Quality, Testing, Security, Performance  
**Output format**:
- ✓ Strengths
- ⚠ Issues (must fix)
- 🔍 Risks (should address)
- 💡 Recommendations (nice to have)
- ✅ Readiness Assessment: Ready to Merge / Needs Changes / Needs Rework

### `doc-sync` — Documentation Sync Agent (Auto, not user-invocable)
**Purpose**: Prevents documentation drift by automatically updating docs when code changes.

**Source of truth hierarchy**: Implementation → Existing docs → ADRs  
**Documents it maintains**: `backend-architecture.md`, `frontend-architecture.md`, `api-specification.md`, `database-design.md`, `assumptions-and-constraints.md`, `production-readiness-checklist.md`  
**ADR creation**: Creates a new `docs/decisions/ADR-XXX-title.md` for every significant architectural decision

---

## 13. Copilot Skills

Five **skill files** in `.github/skills/` serve as reference libraries that agents consult. They contain project-specific patterns, conventions, and best practices.

| Skill File | Purpose |
|-----------|---------|
| `dotnet-skill.md` | ASP.NET Core patterns, EF Core conventions, validation patterns for SkyRoute |
| `angular-skill.md` | Angular 21 standalone component patterns, signals, reactive forms for SkyRoute |
| `architecture-skill.md` | Layered architecture guidance, dependency rules, extensibility patterns |
| `review-skill.md` | Code review standards, requirement compliance criteria, security checklist |
| `testing-skill.md` | Testing patterns, coverage expectations, SkyRoute-specific test focus areas |

**How skills connect to agents**:
- `@backend` → reads `dotnet-skill.md`
- `@frontend` → reads `angular-skill.md`
- `@architect` → reads `architecture-skill.md`
- `@reviewer` → reads `review-skill.md`
- `@tester` → reads `testing-skill.md`

---

## 14. Copilot Instructions File

The most important AI configuration file is `.github/copilot-instructions.md`.

This is the **repository-level system prompt** — every Copilot interaction in this repository starts with these instructions loaded automatically.

It defines:

| Section | Content |
|---------|---------|
| **Project Context** | SkyRoute platform, tech stack, purpose |
| **Business Context** | Providers (GlobalAir, BudgetWings), pricing rules, supported features |
| **Development Philosophy** | 12 principles including "implement only what's documented", "prefer readability", "no speculative design" |
| **Architecture Principles** | Thin controllers, service layer for business logic, dependency flow rules |
| **Backend Anti-Patterns** | Explicit list of patterns NOT to introduce (CQRS, MediatR, Repository Pattern, etc.) |
| **Frontend Standards** | Angular 21 + signals + reactive forms standards |
| **Validation Standards** | Document validation rules (domestic vs international), passenger limits |
| **Error Handling** | Fail fast, never expose internals, user-friendly messages |
| **Testing Standards** | Happy path, failure path, validation, edge cases for every feature |
| **Security Standards** | Input validation, never trust client-side only, protect against OWASP top issues |
| **Feature Development Workflow** | Structured 5-step process: identify requirements → implement → verify → suggest tests → identify risks |
| **Definition of Done** | 8-point checklist every feature must pass before being considered complete |

**Why this matters**: Without this file, Copilot would give generic advice. With it, Copilot knows the exact architecture, the exact anti-patterns to avoid, the exact business rules, and the exact standards — effectively acting as a team member who has read the entire project requirements.

---

## 15. Key Design Decisions

### 1. HttpOnly Cookie for JWT (Security)
JWT tokens were intentionally moved from `localStorage` to HttpOnly cookies to eliminate XSS token theft risk. The frontend never accesses the token — the browser sends it automatically.

### 2. Strategy Pattern for Airline Pricing (Extensibility)
Each airline's pricing logic is encapsulated in its own class implementing `IFlightPricingStrategy`. Adding a third airline requires only adding one new class and one DI registration — the rest of the application is unchanged.

### 3. Clean Layered Architecture (Maintainability)
Domain has zero dependencies. Application depends only on Domain. Infrastructure depends on Application. API depends on both. This enforces a strict dependency direction and makes each layer independently testable.

### 4. Angular Signals over NgRx (Simplicity)
State management uses Angular's built-in Signals instead of adding NgRx or similar libraries. The application state is simple enough that a full state management library would add overhead with no benefit.

### 5. Client-Side Sorting Only (Performance)
Flight results sorting is done purely in the browser via `FlightService.sortFlights()`. No additional API calls are made — once results are loaded, sorting is instant and free.

### 6. Dynamic Document Validation (Business Rule)
The booking form dynamically switches between National ID (domestic) and Passport (international) validation based on a `AIRPORT_COUNTRY` map derived from seeded airport data. The backend re-validates this independently — there is no trust of the client's determination of domestic/international.

### 7. 144 Pre-Seeded Flights (Development Experience)
Flights are generated by seed data covering 12 routes × 3 cabin classes × 4 departure times × 2 airlines. This means the app works with realistic data immediately after `dotnet run` — no manual data entry needed.

---

## 16. Development Philosophy

The project followed 10 core principles, enforced through Copilot instructions:

1. **Implement only documented requirements** — no invented features
2. **Do not introduce complexity without justification** — simplicity first
3. **Every implementation must trace back to a requirement** — no speculative design
4. **Prefer readability over abstraction** — maintainability over clever code
5. **Thin controllers, fat services** — clear separation of concerns
6. **Validate at every boundary** — never trust client input alone
7. **Fail fast and fail clearly** — meaningful errors, no silent failures
8. **Test behavior, not implementation** — meaningful assertions, not brittle ones
9. **Secure by default** — HttpOnly cookies, BCrypt, input validation
10. **Documentation reflects code** — doc-sync agent prevents drift

These principles were not just guidelines — they were enforced by the `@reviewer` agent on every feature and embedded in the `copilot-instructions.md` so Copilot always operated within these constraints.

---

*Generated from codebase scan — reflects actual implementation state as of July 2026.*
