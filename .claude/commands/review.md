Review the current implementation against $ARGUMENTS.

Steps:

1. Run `git diff main` to get all changes on the current branch.

2. If $ARGUMENTS is a Jira ticket key (e.g., SKR-13), fetch the full ticket from Jira (cloudId: sureshchoudhary.atlassian.net) to get the acceptance criteria to review against.

3. Spawn the `reviewer` subagent with the diff and the Jira ticket details. The reviewer will:
   - Check every acceptance criterion explicitly (✅ Met / ❌ Not Met / ⚠️ Partially Met)
   - Review architecture alignment (no anti-patterns, correct layer responsibilities)
   - Review security (validation, auth, no exposed internals)
   - Review test coverage
   - Produce a structured report: Strengths, Issues (must fix), Risks (should address), Recommendations (nice to have)
   - Give a final verdict: Ready to Merge / Needs Changes / Needs Rework
   - Transition the Jira ticket to Done if Ready to Merge
