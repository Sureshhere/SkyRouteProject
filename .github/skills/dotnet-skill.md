# .NET Backend Skill

## Purpose

Act as a Senior ASP.NET Core Engineer.

Generate production-quality backend code aligned with project requirements.

---

# Technology Stack

- ASP.NET Core Web API
- .NET 10
- C#
- SQL Server
- Entity Framework Core

---

# Backend Standards

Use:

- Dependency Injection
- DTOs
- Async/Await
- Strong Typing
- Validation
- Structured Logging

Controllers should:

- Accept requests
- Validate requests
- Call services
- Return responses

Controllers should never contain business logic.

---

# Service Layer Standards

Business logic belongs in services.

Services should:

- Have a single responsibility
- Remain testable
- Be dependency-injected
- Contain business rules

---

# API Standards

Use:

- REST conventions
- Meaningful route names
- Appropriate HTTP status codes
- Consistent request models
- Consistent response models

---

# Validation Standards

Validate:

- Required fields
- Business rules
- Input constraints

Prefer centralized validation.

Never rely solely on frontend validation.

---

# Error Handling Standards

Handle failures gracefully.

Return meaningful errors.

Do not expose internal implementation details.

---

# Database Standards

Use EF Core correctly.

Avoid:

- N+1 queries
- Unnecessary database calls
- Duplicate queries

Prefer:

- Explicit relationships
- Proper indexing
- Efficient querying

---

# Code Quality Standards

Generate:

- Readable code
- Maintainable code
- Testable code

Use meaningful naming.

Prefer clarity over brevity.

---

# Anti-Patterns

Do not introduce:

- CQRS
- MediatR
- Repository Pattern
- Unit Of Work
- Event Sourcing
- Microservices

Unless explicitly requested.