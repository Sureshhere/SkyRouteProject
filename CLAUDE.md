# SkyRoute Travel Platform

Angular 21 + ASP.NET Core .NET 8 + SQL Server. Full architecture docs in `/docs`.

## Architecture

Clean layered — dependencies flow inward only:

```
SkyRoute.Api → SkyRoute.Application → SkyRoute.Infrastructure → SkyRoute.Domain
```

Domain has zero external dependencies. Business logic lives in services. Controllers only orchestrate (receive → validate → call service → return response).

## Tech Stack

**Backend**: ASP.NET Core Web API, .NET 8, C#, EF Core 8 Code-First, FluentValidation, BCrypt, JWT HttpOnly cookie, xUnit + Moq + FluentAssertions  
**Frontend**: Angular 21, standalone components, signals, reactive forms, RxJS, Karma + Jasmine  
**Database**: SQL Server (LocalDB in dev), 2 airlines (GlobalAir, BudgetWings), 6 airports, 144 seeded flights

## Business Rules

- Passengers: 1–9 per booking
- Domestic flights (same country): National ID required
- International flights (different countries): Passport required
- Backend independently validates domestic/international — never trusts client
- Booking reference format: `SK{yyyyMMdd}{6-hex}`
- JWT stored as HttpOnly cookie — never in localStorage, never readable by JS
- GlobalAir pricing: `base × 1.15`
- BudgetWings pricing: `max(base × 0.90, 29.99)`

## Development Rules

- Implement only documented requirements — no invented features
- Every decision traces back to a requirement
- Prefer simplicity and readability over abstraction
- Validate at every boundary — never trust client input alone
- Fail fast, fail clearly — no silent failures

**Backend — never introduce**: CQRS, MediatR, Repository Pattern, Unit of Work, Event Sourcing, Message Brokers, Microservices patterns

**Frontend — never introduce**: NgRx or external state libraries. Use Angular signals. No `any` in TypeScript.

**Every frontend feature must have**: loading state, error state, empty state.

## Jira Integration

Project key: `SKR` | Site: `sureshchoudhary.atlassian.net`  
Use Atlassian MCP tools to fetch tickets, transition statuses, and add comments.

## Agents — `.claude/agents/`

| Agent | Use for |
|-------|---------|
| `architect` | Design before implementing — API contracts, data models, tradeoffs. No code. |
| `backend` | ASP.NET Core implementation — controllers, services, DTOs, EF Core, validation |
| `frontend` | Angular implementation — components, forms, signals, services, routing |
| `tester` | xUnit backend tests and Karma/Jasmine frontend tests |
| `reviewer` | Code review as Principal Engineer — requirements, architecture, security |
| `doc-sync` | Keep `/docs` in sync after any code change |

## Slash Commands — `.claude/commands/`

| Command | Purpose |
|---------|---------|
| `/implement <SKR-XX>` | Fetch Jira ticket and implement it end-to-end |
| `/architect <SKR-XX or description>` | Design a feature before implementing |
| `/review <SKR-XX>` | Review current changes against Jira AC and architecture |
| `/sync-docs` | Sync `/docs` after implementation |
