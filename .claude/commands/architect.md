Design the feature described by: $ARGUMENTS

If $ARGUMENTS is a Jira ticket key (e.g., SKR-13), fetch the full ticket from Jira first (cloudId: sureshchoudhary.atlassian.net) to get the requirements and acceptance criteria.

Spawn the `architect` subagent to produce an implementation-ready design. The design must include:

1. Requirements summary — what needs to be built and why
2. Impact analysis — which backend layers, frontend components, and database tables are affected
3. API contracts — full endpoint specs with method, path, request schema, response schema, auth requirement, and status codes
4. Data model — any new entities or schema changes with EF Core migration notes
5. Implementation roadmap — ordered steps for the backend and frontend agents
6. Risks — what could go wrong and how to mitigate it

Do not write any code. Design only. Post the design as a Jira comment if a ticket key was provided.
