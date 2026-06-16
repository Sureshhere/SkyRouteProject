# Project Context

Project Name: SkyRoute Travel Platform

Purpose:

Build a Flight Search and Booking module for a travel aggregation platform.

Technology Stack:

Backend:
- ASP.NET Core Web API
- C#
- SQL Server

Frontend:
- Angular 21
- Standalone Components
- Signals
- Reactive Forms

This is a technical assessment project.

The goal is to satisfy all documented requirements while demonstrating production-quality engineering practices.

The solution should remain maintainable, readable, testable, and extensible.

Future airline providers may be added to the platform.

---

# Business Context

SkyRoute aggregates flight data from multiple airline providers.

Current providers:

- GlobalAir
- BudgetWings

Each provider has independent pricing rules.

Additional providers may be added in the future.

The application must support:

- Flight Search
- Flight Results
- Client-side Sorting
- Booking
- Booking Confirmation

Document requirements vary based on domestic versus international travel.

---

# Development Philosophy

Follow these principles:

1. Implement only documented requirements.

2. Do not invent features.

3. Do not introduce complexity without justification.

4. Prefer simple solutions.

5. Prefer readability over abstraction.

6. Prefer maintainability over clever code.

7. Every implementation must trace back to a requirement.

8. Avoid speculative design.

9. Build only what is needed now.

10. Explain reasoning before implementation.

11. Keep the codebase approachable for future developers.

12. Optimize for clarity first, extensibility second.

---

# Requirement Compliance

Requirements are the source of truth.

Always align implementation with:

- Challenge requirements
- Acceptance criteria
- Architecture decisions

If a requirement is unclear:

- Identify the ambiguity
- Explain assumptions
- Recommend the safest implementation

Never silently invent behavior.

---

# Architecture Principles

Use separation of concerns.

Controllers:

- Accept requests
- Validate requests
- Call services
- Return responses

Services:

- Contain business logic

DTOs:

- Define API contracts

Models:

- Represent business concepts

Provider implementations:

- Encapsulate provider-specific behavior

Dependencies should flow inward.

Prefer interface-based abstractions where they improve maintainability.

Avoid introducing architectural patterns that do not provide immediate value.

The architecture should remain simple while allowing new airline providers to be added with minimal code changes.

---

# Backend Standards

Use:

- Dependency Injection
- DTOs
- Async/Await
- Validation
- Typed Responses

Controllers must remain thin.

Business logic belongs in Services.

Validation should be centralized.

Use meaningful names.

Generate production-quality code.

Prefer explicit implementations over clever abstractions.

Use configuration over hardcoded values whenever appropriate.

Do not expose internal models directly through APIs.

---

# Backend Anti-Patterns

Do not introduce:

- CQRS
- MediatR
- Repository Pattern
- Unit Of Work
- Event Sourcing
- Message Brokers
- Distributed Systems
- Microservices
- Event-Driven Architecture

Unless explicitly requested.

The challenge scope does not justify these patterns.

Prefer simplicity.

---

# Frontend Standards

Use:

- Angular 21
- Standalone Components
- Signals
- Reactive Forms
- Strong Typing

Prefer feature-based organization.

Use strongly typed models.

Keep components focused.

Separate presentation logic from API communication.

Avoid deprecated Angular patterns.

Avoid excessive state management solutions.

Use Angular's modern capabilities before introducing additional libraries.

---

# Frontend UX Standards

Always provide:

- Loading State
- Error State
- Empty State

Forms should:

- Display validation messages
- Prevent invalid submission
- Provide clear feedback

Sorting should happen client-side where required.

The UI should clearly distinguish:

- Total Price
- Per Passenger Price

---

# Provider Design Guidelines

Provider implementations must remain isolated.

Each provider is responsible for:

- Flight generation
- Pricing calculations
- Provider-specific behavior

New providers should be added by:

1. Creating a new provider implementation
2. Registering it with dependency injection

Existing providers should not require modification.

Follow the Open/Closed Principle where practical.

---

# Validation Standards

Always validate:

- Required fields
- Passenger count limits
- Email format
- Document number rules
- Search request integrity

Business rules must be enforced consistently.

Validation logic should be reusable.

Domestic flights:

- National ID

International flights:

- Passport Number

Validation must adapt to route type.

---

# Error Handling Standards

Handle failures gracefully.

Return meaningful error messages.

Do not expose internal implementation details.

Validate inputs early.

Fail fast when invalid requests are detected.

Provide user-friendly responses.

---

# Testing Standards

For every feature identify:

- Happy Paths
- Failure Paths
- Validation Scenarios
- Edge Cases

Focus testing on business behavior.

Prioritize testing:

- Pricing calculations
- Booking rules
- Document validation
- Search behavior

Avoid brittle tests.

Prefer maintainable tests.

---

# Performance Standards

Avoid premature optimization.

Focus on:

- Clean APIs
- Efficient data handling
- Avoiding unnecessary API calls

Client-side sorting should not trigger backend requests.

Keep implementations efficient and understandable.

---

# Security Standards

Validate all inputs.

Never trust client-side validation alone.

Protect against:

- Invalid input
- Data exposure
- Unexpected request payloads

Follow secure coding practices.

Avoid storing sensitive information unnecessarily.

---

# Feature Development Workflow

Before implementation:

1. Identify requirements covered.
2. Identify acceptance criteria covered.
3. Identify impacted files.
4. Explain design decisions.
5. Explain validation requirements.
6. Identify dependencies.

Then implement.

After implementation:

1. Verify acceptance criteria.
2. Verify validation rules.
3. Verify edge cases.
4. Suggest tests.
5. Identify risks.

---

# Code Review Workflow

Review generated code for:

- Requirement coverage
- Correctness
- Maintainability
- Security
- Validation
- Readability
- Simplicity

Identify:

- Risks
- Missing requirements
- Potential improvements

Do not suggest unnecessary complexity.

Prefer practical improvements.

---

# Documentation Standards

Keep documentation current.

Document:

- Architectural decisions
- API contracts
- Business rules
- Assumptions

Documentation should help future developers understand the reasoning behind implementation decisions.

---

# Definition Of Done

A feature is complete only when:

- Requirements are implemented
- Acceptance criteria are satisfied
- Validation is implemented
- Error handling is implemented
- Edge cases are considered
- Tests are identified
- Code review passes
- Documentation is updated when necessary

Code generation should always optimize for:

- Correctness
- Readability
- Maintainability
- Requirement compliance