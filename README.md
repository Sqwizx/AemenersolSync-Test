# Aemenersol Platform/Well Sync Dashboard

A full-stack sync application: an ASP.NET Core 8 Web API logs into the Aemenersol test API,
pulls Platform/Well data, and upserts it into a SQL Server LocalDB database via EF Core
Code First. A React dashboard provides a manual "Sync Now" trigger, and a background
scheduler keeps the data fresh automatically, pushing live updates to the browser over
SignalR.

## Prerequisites

- .NET 8 SDK
- SQL Server Express LocalDB (`MSSQLLocalDB` instance)
- Node.js 18+ and npm

## Running the backend

```bash
cd backend/AemenersolSync.Api
dotnet ef database update   # creates the AemenersolSync LocalDB database
dotnet run
```

The `https` launch profile (`backend/AemenersolSync.Api/Properties/launchSettings.json`)
serves on `https://localhost:7208` (and `http://localhost:5281`). The frontend's
`frontend/src/api/client.js` (`BASE_URL`) and `frontend/src/api/signalr.js` (`HUB_URL`)
are already pointed at `https://localhost:7208` to match. If your machine assigns a
different port, update those two constants accordingly.

Aemenersol credentials and the API base URL are read from `appsettings.json`
(`Aemenersol:BaseUrl`/`Username`/`Password`) — there are no credential fields in the
login UI; the backend logs in with this fixed server-side configuration on the user's
behalf.

## Running the frontend

```bash
cd frontend
npm install
npm run dev
```

Visit `http://localhost:5173`. The backend's CORS policy (in `Program.cs`) only allows
this origin, with credentials, so the frontend must be served from exactly this URL for
the session cookie to work.

## Running tests

```bash
cd backend/AemenersolSync.Api.Tests
dotnet test
```

20 xUnit tests, all passing, covering: DTO deserialization against both the real
aemenersol schema and its "Dummy" schema-drift variant, upsert null-preservation
semantics, `SyncService` orchestration (including the unauthorized/token-clear path),
the background scheduler's tolerance of auth failures, EF Core model configuration
(`ValueGeneratedNever` on the externally-sourced `Id` columns), and JSON serialization
cycle prevention (`Well.Platform` is `[JsonIgnore]`d to avoid a Platform → Wells →
Platform loop when `GET /api/sync/data` serializes the graph).

## What this demonstrates

The assessment's literal requirements are: login + fetch via bearer token, upsert-by-id
into EF Core Code First tables, and defensive deserialization that survives the API
gaining or losing fields (verified against `GetPlatformWellDummy`). Those are implemented
in `backend/AemenersolSync.Api/Services/PlatformWellRepository.cs` (upsert) and
`backend/AemenersolSync.Api/Dtos/` (nullable DTOs + null-preserving update mapping).

Everything else — the React dashboard, SignalR, the background scheduler, the login
modal, cookie-based session handling — is an additive choice to demonstrate full-stack
range for a full-stack role, not something the assessment itself requires.

## Key design decisions

- **Upsert semantics:** a field missing from an API response does not overwrite the
  existing database value on update (it's treated as "unknown," not "now empty"). See
  `PlatformWellRepository.ApplyPlatformFields`/`ApplyWellFields`, which only assign a DTO
  field onto the entity when it's non-null; a missing/invalid `Id` (or Well's
  `PlatformId`) is logged and the record skipped entirely, since there's no key to
  resolve an update against. Verified with `GetPlatformWellDummy`, which replaces
  `createdAt`/`updatedAt` with a new `lastUpdate` field — per the task's own instruction
  ("not required to handle the newly added key, just handle the same key on the
  original dataset"), `lastUpdate` is ignored and `createdAt`/`updatedAt` are treated as
  missing (null-on-insert, preserved-on-update).
- **Repository pattern covers both reads and writes:** `IPlatformWellRepository`/
  `PlatformWellRepository` is the single data-access seam for the Platform/Well tables.
  `SyncService` writes through it (`UpsertPlatformsAsync`), and `SyncController.GetData`
  reads through it (`GetAllPlatformsWithWellsAsync`) rather than touching `AppDbContext`
  directly. This was revisited mid-project: the repository was briefly removed in favor
  of direct `AppDbContext` access, then restored — more consistently than the original
  design, which had only writes going through a repository while reads hit the
  `DbContext` directly. Both now go through the same interface.
- **Token handling:** the Aemenersol bearer token never reaches the browser. The backend
  holds it in a single in-memory slot (`InMemorySyncTokenCache`, wrapped by
  `SessionSyncTokenStore`/`ISyncTokenStore` — a deliberate simplification for this
  single-user demo) and the browser only ever sees an opaque, `HttpOnly`/`Secure`/
  `SameSite=Strict` session cookie.
- **No message broker:** Kafka/RabbitMQ were considered and rejected — there is no real
  external event to justify a broker here, and it would require the reviewer to stand up
  infrastructure just to run the submission. A `BackgroundService` timer
  (`SyncBackgroundService`) + SignalR push (`SyncHub`) achieves the same "automatically
  refreshing dashboard" experience with zero extra infrastructure.

## Known limitations

- The Aemenersol API is HTTP-only, so the login payload and bearer token travel
  unencrypted between this backend and that server. This is outside our control.
- The in-memory token/session store is a single global slot — correct for a single-user
  demo, not safe for multiple concurrent users (see "Future recommendations" below).
- A stolen session cookie can act as the logged-in user against this backend — inherent
  to any session mechanism, mitigated (not eliminated) via `HttpOnly`/`Secure`/
  `SameSite=Strict` and a 30-minute idle timeout.
- "Preserve existing value when a field is missing" solves data *completeness* but not
  data *freshness* — the app doesn't distinguish a value that's current from one that's
  silently gone stale because the upstream API stopped returning that field for a given
  record. Harmless for rarely-changing fields like `uniqueName`/coordinates; would
  matter more for anything safety- or operationally-relevant in a real deployment.

## Future recommendations (not implemented)

- **Per-field data freshness tracking** — today, a value's absence from an API response
  means "keep the old value," with no record of how stale that value has become. A
  production version of this (e.g. for an actual mining site) would want per-field
  "last known good" timestamps (or a staleness threshold + visual flag in the UI) so
  viewers can distinguish a live reading from a silently-stale one.
- **Multi-tenancy** — replacing the single global session/token slot with per-user
  identity, scoping the background scheduler per tenant, and partitioning the
  Platform/Well tables by tenant/site. This would also change the primary key strategy:
  today `Platform.Id`/`Well.Id` directly reuse aemenersol's own ids (correct for a
  single source, single tenant), but multiple tenants could have colliding upstream ids,
  requiring an independent auto-increment PK plus a `(TenantId, ExternalId)` unique pair.

## Time spent

