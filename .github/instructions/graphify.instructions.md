---
applyTo: "**"
---

# graphify (mandatory first step for codebase questions)

When `graphify-out/graph.json` exists, `graphify query "<question>"` is your FIRST tool call for
any question about this repo's architecture, structure, components, or how to add/modify/find
code — before `file_search`, `grep_search`, `read_file`, or answering from memory. This applies
even when you are confident you already know the answer: confidence from recall is exactly the
case this rule exists to override. Do not substitute a quick `file_search` "verification" for
calling `graphify query` — call it directly.

Use `graphify path "<A>" "<B>"` for relationship questions and `graphify explain "<concept>"` for
focused-concept questions.
