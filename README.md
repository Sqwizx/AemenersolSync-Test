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

Credentials and the API base URL are read from `appsettings.json`
(`Aemenersol:BaseUrl`/`Username`/`Password`) — there are no credential fields in the
login UI; the backend logs in with this fixed server-side configuration on the user's
behalf.

A Swagger UI is also available while running in Development, at
`https://localhost:7208/swagger/index.html` — useful for exercising the API endpoints
directly (login → sync → data) without the React frontend.

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

20 xUnit tests, covering: DTO deserialization against both the real
provided API schema and its "Dummy" example - upsert null-preservation
semantics, `SyncService` orchestration (including the unauthorized path),
the background scheduler's tolerance of auth failures, EF Core model configuration, and JSON serialization cycle prevention (`Well.Platform` is ignored `[JsonIgnore]` to avoid a Platform → Wells →
Platform loop when `GET /api/sync/data` serializes the graph).

## Key design decisions

- **Upsert semantics:** a field missing from the provided API response does not overwrite the
  existing database value on update (it's treated as "unknown,"). And only assigns a DTO
  field onto the entity when it's non-null; a missing/invalid `Id` (or Well's
  `PlatformId`) is logged and the record skipped entirely.
- **Token handling:** the upstream API bearer token never reaches the browser. The backend
  holds it in a single in-memory slot (`InMemorySyncTokenCache`, wrapped by
  `SessionSyncTokenStore`/`ISyncTokenStore` — a deliberate simplification for this
  single-user demo) and the browser only ever sees an opaque, `HttpOnly`/`Secure`/
  `SameSite=Strict` session cookie.

## Known limitations

- The provided API is HTTP-only, so the login payload and bearer token travel
  unencrypted between this backend and that server. Which is expected.
- The in-memory token/session store is a single global slot is intentional for a single-user
  demo, not suitable for multiple concurrent users (see "Future recommendations" below).
- A stolen session cookie can act as the logged-in user against this backend, mitigated via `HttpOnly`/`Secure`/
  `SameSite=Strict` and a 30-minute idle timeout.

## Future recommendations

- **Per-field data freshness tracking** — a value's absence from an API response
  means "keep the old value," with no record of how stale that value has become. A
  production version of this would want per-field
  "last known good" timestamps (or a staleness threshold + visual flag in the UI) so
  viewers can distinguish a live reading from a silently-stale one.
- **Multi-tenancy** — replacing the single global session/token slot with per-user
  identity, scoping the background scheduler per tenant, and partitioning the
  Platform/Well tables by tenant/site. This would also change the primary key strategy:
  Currently, `Platform.Id`/`Well.Id` directly reuse the provided ids, but multiple tenants could have colliding upstream ids,
  requiring an independent auto-increment PK plus a `(TenantId, ExternalId)` unique pair.

## Time spent

~12 hours total:

| Area | Hours | Notes |
|---|---|---|
| MVP (EF Core Code First, login → fetch → upsert, defensive deserialization, controllers) | 5.5 | entity/migration design, the provided HTTP client with null-safe DTOs, the upsert-by-id repository, auth/sync controllers. Includes debugging time against the live API and LocalDB. |
| Additional Feature | 1.5 | Background Scheduler (token cache, SignalR server-side wiring) |
| Frontend | 3.5 | React scaffold, dashboard, login modal, table, SignalR client, styling. |
| Tests | 1.5 | Added service tests and two regression tests added specifically because bugs were found during development - IDENTITY/JSON-cycle bugs had no coverage until found. |
| **Total** | **12** | Besides MVP development time and debugging time where some bugs were found during the development  |
