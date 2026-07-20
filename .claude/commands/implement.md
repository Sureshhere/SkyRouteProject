Implement Jira ticket $ARGUMENTS from the SkyRoute project using parallel agents.

--- PHASE 1: Design (sequential — everything depends on this) ---

Fetch the ticket from Jira (cloudId: sureshchoudhary.atlassian.net) to get the full description and acceptance criteria.

Then spawn the `architect` subagent. Wait for it to complete before proceeding.
The architect must produce: API contracts, data model changes, validation strategy, and an ordered implementation roadmap.

--- PHASE 2: Implementation (parallel — run both at the same time) ---

Once the architect's design is complete, spawn the `backend` subagent and the `frontend` subagent IN PARALLEL in a single step — do not wait for one before starting the other.

- `backend`: implement all backend changes (Domain → Application → Infrastructure → Api), including EF Core migration if schema changed
- `frontend`: implement all Angular changes (components, services, forms, routing) — every UI feature must have loading, error, and empty states

Wait for BOTH to complete before proceeding.

--- PHASE 3: Tests (sequential — needs implementation complete) ---

Spawn the `tester` subagent. Wait for it to complete.
Must cover: happy path, validation failures, business rule edge cases for both backend (xUnit) and frontend (Karma/Jasmine).

--- PHASE 4: Wrap-up (parallel — run both at the same time) ---

Spawn the `doc-sync` subagent and the `reviewer` subagent IN PARALLEL in a single step.

- `doc-sync`: update all affected docs in /docs to reflect the implementation
- `reviewer`: fetch the Jira ticket $ARGUMENTS, verify every acceptance criterion is met, transition ticket to Done on approval

Wait for BOTH to complete, then report the final outcome.
