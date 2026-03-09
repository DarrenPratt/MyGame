# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Architecture, design decisions, scope | Johnny | System design, C# project structure, trade-offs, what to build next |
| C# implementation, game engine, core systems | Judy | Game loop, command parser, room/item models, player state, save/load |
| Story, world content, narrative text | Rogue | Room descriptions, item flavor text, NPC dialogue, storyline |
| Code review | Johnny | Review PRs, check quality, suggest improvements |
| Testing | River | Write xUnit tests, find edge cases, verify fixes |
| Scope & priorities | Johnny | What to build next, trade-offs, decisions |
| Session logging | Scribe | Automatic — never needs routing |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, assign `squad:{member}` label | Johnny |
| `squad:johnny` | Architecture or lead tasks | Johnny |
| `squad:judy` | C# implementation tasks | Judy |
| `squad:rogue` | Content and narrative tasks | Rogue |
| `squad:river` | Testing and QA tasks | River |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, **Johnny** triages it — analyzing content, assigning the right `squad:{member}` label, and commenting with triage notes.
2. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
3. Members can reassign by removing their label and adding another member's label.
4. The `squad` label is the "inbox" — untriaged issues waiting for Johnny's review.

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for "what is the command list?"
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** If a feature is being built, spawn River to write test cases from requirements simultaneously.
7. **Issue-labeled work** — when a `squad:{member}` label is applied to an issue, route to that member. Johnny handles all `squad` (base label) triage.
