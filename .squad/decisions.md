# Squad Decisions

## Active Decisions

### ColorConsole Design (Issue #3) — 2026-03-10

**Semantic colour palette (cyberpunk theme)**
| Role | Method | ANSI code | Rationale |
|---|---|---|---|
| Room descriptions | `RoomDescription` | `\x1b[96m` bright cyan | Environment is the neon world — should glow |
| Errors | `Error` | `\x1b[31m` red | Hard stop, immediate danger |
| Input prompt | `Prompt` | `\x1b[2;36m` dim cyan | Quietly waiting, doesn't compete with output |
| Flavour / atmosphere | `Flavor` | `\x1b[35m` magenta | Cyberpunk neon sign colour |

Existing primitives (`Cyan`, `BoldCyan`, `Yellow`, `Green`, `Magenta`, `DarkGray`) retained unchanged — backwards compatibility.

**Windows ANSI support**
Used `kernel32.dll` P/Invoke (`GetStdHandle`/`GetConsoleMode`/`SetConsoleMode`) with `ENABLE_VIRTUAL_TERMINAL_PROCESSING (0x0004)`. Guarded with `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)`. Graceful degradation on unsupported terminals.

**Initialization**
`Initialize()` called from `Program.cs` top-level statements as the first line before any console output. Tests unaffected (ANSI codes are transparent to `OutputContains` substring assertions).

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
