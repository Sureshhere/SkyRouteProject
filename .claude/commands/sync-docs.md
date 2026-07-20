Sync the /docs directory to reflect all recent code changes.

Steps:

1. Run `git diff main` to identify all code changes on the current branch.

2. Spawn the `doc-sync` subagent with the diff. The agent will:
   - Identify which documents in /docs are affected by the changes
   - Update api-specification.md if any API endpoints were added, changed, or removed
   - Update database-design.md if any entities or schema were changed
   - Update backend-architecture.md if any services, patterns, or integrations changed
   - Update frontend-architecture.md if any components, routes, or state patterns changed
   - Create an ADR in /docs/decisions/ if a significant architecture decision was made
   - Remove any documentation that no longer reflects the implementation

Do not create new documents unless no existing document covers the changed area.
