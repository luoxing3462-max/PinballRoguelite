# Cache Ownership

- Cache/temp-* is isolated disposable support for one task, test, build, or verification unit.
- Allocate a fresh run before task-support writes. Final source, assets, and requested deliverables remain outside Cache.
- Delete each run after its verified result is handed off; retain only exact active or recovery inputs with a named consumer and ending condition.
- current.json is a generated, replaceable operational snapshot. It is not project memory or history and must remain ignored.
- Do not place Unity's native Library, Temp, Logs, or canonical runtime assets under this owner.
- Codex chats and session data are user-managed and excluded from Cache cleanup.