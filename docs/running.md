# Running locally

[← README](../README.md) · [Running](running.md) · [Testing](testing.md) · [API](api.md) · [Interpretation](interpretation.md) · [Decisions](decisions.md) · [Architecture](architecture.md) · [Dashboard](dashboard.md) · [Deferred](deferred.md)

Prerequisites, configuration, running and stopping the app, and troubleshooting.

## Prerequisites

| Need | Version | Notes |
|---|---|---|
| .NET SDK | **10.0.201** (`global.json`, `rollForward: latestFeature`, so any 10.0.2xx or later feature band works) | Target framework `net10.0` (`Directory.Build.props`). `global.json` selects the Microsoft.Testing.Platform test runner |
| Node.js | `^22.22.3`, `^24.15.0` or `>=26.0.0`, the engines range of Angular 22.2 (`web/package-lock.json`). Verified with v26.0.0 | `web/package.json` declares no `engines` of its own |
| npm | **11.12.1** (`"packageManager": "npm@11.12.1"` in `web/package.json`) | `npm ci` installs from the lockfile |
| Docker | Any recent Docker Desktop or Engine with Compose v2. Verified with 29.5.3 | It runs the SQL Server container for the app. **The Infrastructure and API test suites also need Docker running**, because Testcontainers starts its own SQL Server containers |
| Global Angular CLI | **Not needed** | `npm start` and `npm test` run the project-local `ng` from `web/node_modules` |
| `dotnet-ef` tool | **Not needed to run** | The API applies migrations on startup in Development. You need `dotnet-ef` only to list or author migrations (see [EF Core commands](#ef-core-commands)); the repo has no tool manifest |

**Ports**

| Port | Used by | Set in |
|---|---|---|
| 1433 | SQL Server (host side) | `RELAY_DB_PORT` in `.env`, default 1433 (`docker-compose.yml`) |
| 5080 | API | `src/Relay.Api/Properties/launchSettings.json` (`applicationUrl`) and the dev proxy target in `web/proxy.conf.json` |
| 4200 | Angular dev server | Angular default (`ng serve`; `angular.json` sets no port) |

**Apple Silicon:** the SQL Server 2022 image has no arm64 build. `docker-compose.yml` sets `platform: linux/amd64`, so it runs under Rosetta. Enable "Use Rosetta for x86_64/amd64 emulation" in Docker Desktop. The first start is slower.

---

## Configuration

Nothing secret is committed. `.env` is git-ignored, and `.env.example` holds only a placeholder password. Never put a real password in `.env.example`.

One file, `.env` at the repo root, configures both the database container and the API. Docker Compose reads it automatically. In Development the API reads it too, through [DotNetEnv](https://github.com/tonerdo/dotnet-env) (PLAN §13, "OS-agnostic local configuration via DotNetEnv"): it looks for `.env` in the content root and its parent folders, fills in `${…}` references, and adds the values to .NET configuration. It does not change the process environment. Nothing needs exporting, so the steps are the same in any shell and on any OS.

| Variable | Purpose | Default | Read by |
|---|---|---|---|
| `RELAY_DB_SA_PASSWORD` | SQL Server `sa` password | **None, required.** Compose stops with `set RELAY_DB_SA_PASSWORD in .env` if it is missing | `docker compose`, which reads `.env` automatically and passes it to the container as `MSSQL_SA_PASSWORD`. It is also filled into `ConnectionStrings__Relay` |
| `RELAY_DB_PORT` | Host port for SQL Server | `1433` | `docker compose` (port mapping); filled into `ConnectionStrings__Relay` |
| `ConnectionStrings__Relay` | API connection string (`ConnectionStrings:Relay` in .NET configuration) | Built in `.env.example` from the two variables above: `Server=localhost,${RELAY_DB_PORT};Database=relay;User Id=sa;Password=${RELAY_DB_SA_PASSWORD};TrustServerCertificate=True` | The API (it is in no `appsettings*.json`): in Development from `.env`, or from a real environment variable, which wins (see [Precedence](#precedence)). `dotnet ef` reads it the same way, because it runs with `--startup-project src/Relay.Api` |
| `ASPNETCORE_ENVIRONMENT` | Environment name | `Development`, set by `launchSettings.json` for `dotnet run` | The API. Migrate-on-start and the missing-connection-string check run **only in Development**. In Production a missing connection string surfaces as a 500 on the first request |

The tests need none of these. Testcontainers creates its own databases and connection strings.

### Password rules
- SQL Server's `sa` complexity rules apply. It rejects weak passwords, and the container then exits. Use at least 8 characters from three of the four groups: upper case, lower case, digits, symbols.
- A password containing `$` must be **single-quoted** in `.env`, e.g. `RELAY_DB_SA_PASSWORD='Pa$w0rd…'`. Without the quotes, `${…}` expansion (by both Compose and DotNetEnv) mangles it.
- Avoid `"` and `;` (`;` ends a value in the connection string).
- Keep the variable order of `.env.example`: a `${VAR}` reference must be defined **above** the line that uses it, so `RELAY_DB_SA_PASSWORD` and `RELAY_DB_PORT` stay above `ConnectionStrings__Relay`.

### Precedence
- A real environment variable overrides the same key in `.env`. For example, `ConnectionStrings__Relay` set in your shell, IDE or CI replaces the value built from `.env`.
- Production never reads `.env`; there the connection string must come from the environment (or another configuration source).

**Changing the password or resetting the data:** the `relay-db-data` volume keeps the password it was created with. Changing `.env` later does **not** change it. To reset, run `docker compose down -v` (this **deletes the database**), then `docker compose up -d --wait db`. The next API start recreates and reseeds the database.

---

## Run the app

### One command (macOS, Linux)

After steps 1 and 2 below (`.env` with a password), run `scripts/dev.sh`. It works from any directory: it changes to the repo root itself.

- If `.env` is missing, it exits with `Copy .env.example to .env and set RELAY_DB_SA_PASSWORD first.`
- If something already answers on port 5080 or 4200, it exits with `Port 5080 is already in use; stop the other API first.` (or the same for 4200 and the web server).
- It starts the DB with `docker compose up -d --wait db` and waits until it is healthy.
- On the first run (no `web/node_modules`), it runs `npm ci` in `web`.
- It starts the API and waits up to 120 s for `http://localhost:5080/api/accounts` to answer; it exits with a message if the API stops or never answers.
- It then prints `Open http://localhost:4200` and serves the web app there (`/` redirects to `/dashboard`).
- Ctrl+C stops the API and web. The DB keeps running: stop it with `docker compose down`, or `docker compose down -v` to wipe the data.

### Step by step (Windows, or any OS)

The same steps on macOS, Linux and Windows, from the repo root.

1. **Configure:** `cp .env.example .env` (on Windows: `copy .env.example .env`).
2. **Password:** set `RELAY_DB_SA_PASSWORD` in `.env` (see the password rules above).
3. **Database:** `docker compose up -d --wait db`. This returns when the container's `sqlcmd` health check passes (it retries for up to about 3 minutes).
4. **API** (terminal 1): `dotnet run --project src/Relay.Api`.
   It listens on **http://localhost:5080**. On the first start in Development it creates the `relay` database, applies `InitialCreate` (the schema) and `LoadSeedData` (runs `db/seed.sql`, about 12.6k rows, in about 1 s). It logs `Applying Relay database migrations on start (Development)`, and the port opens only after migration finishes, so the first start takes a few seconds longer. If the connection string is missing, it stops at startup with:
   `The connection string 'ConnectionStrings:Relay' is missing or empty. Copy .env.example to .env at the repository root (or set the environment variable 'ConnectionStrings__Relay') before starting the API.`
   Quick check: `curl -s http://localhost:5080/api/accounts` returns 20 accounts.
5. **Web** (terminal 2): `cd web && npm start`. The first time, run `npm ci` in `web` before it. `ng serve` proxies `/api` to `http://localhost:5080` (`web/proxy.conf.json`).

Then open **http://localhost:4200/dashboard**.

| URL | What |
|---|---|
| http://localhost:4200/dashboard | The dashboard (`/` redirects here) |
| http://localhost:5080/api/accounts | The account list (20 accounts) |
| http://localhost:5080/api/accounts/14/activity-health | Account 14, latest complete week ([api.md](api.md)) |

**What you should see.** The URL is rewritten to `/dashboard?account=14&week=2026-07-20&type=all`:
- "Viewing as" shows **Beacon Home Security** (account 14).
- The week control reads **Mon Jul 20 – Sun Jul 26, 2026**. Clicking it opens the week picker, where only Mondays from Mon Jan 26 to Mon Jul 20, 2026 can be chosen.
- The summary shows 26 inbound events, usually 18–38 a week, within the usual range.
- The first row of the locations table is **Site B**: 2 events, "Usually 3–12 a week", **"▼ Lower than usual"**. It is followed by Sites C, A and D, all "Within usual range".
- The footer, "About these numbers", shows **Data as of Mon Jul 27, 2026**.

Other views worth opening:

| View | URL | What it shows |
|---|---|---|
| The spike | `?account=6&week=2026-06-01` | 880 events; all 15 sites "▲ Higher than usual" |
| After the spike | `?account=6&week=2026-07-20` | 87 events, still within the usual range |
| Empty account | `?account=20` | The empty state |
| Early history | `?account=8&week=2026-03-02` | "Not enough history yet" |

Reloading any of these reproduces the same view.

**Stop and reset**

| To | Run |
|---|---|
| Stop the API or the web server | `Ctrl+C` in its terminal |
| Stop the DB and keep the data | `docker compose stop db` (or `docker compose down`) |
| Delete the DB and its data | `docker compose down -v`. The next API start reseeds it |

---

## EF Core commands

The API applies migrations on start in Development, so you only need these to inspect or author migrations. Install the tool once with `dotnet tool install --global dotnet-ef`. There is no design-time factory: `dotnet ef` builds the `Relay.Api` host, which reads `.env` exactly as `dotnet run` does.

| To | Run (repo root) |
|---|---|
| Apply migrations to the database | `dotnet ef database update --project src/Relay.Infrastructure --startup-project src/Relay.Api` |
| List migrations and whether each is applied | `dotnet ef migrations list --project src/Relay.Infrastructure --startup-project src/Relay.Api` |
| Add a migration | `dotnet ef migrations add <Name> --project src/Relay.Infrastructure --startup-project src/Relay.Api` |

On a fresh clone, run `dotnet build` once first; otherwise `dotnet ef` fails with `NETSDK1004: Assets file … not found`.

`migrations add` needs no database. `database update` and `migrations list` connect to it, so start it first with `docker compose up -d --wait db`.

---

## Troubleshooting

| Symptom | Cause and fix |
|---|---|
| The API won't bind, or the web proxy gets errors on port 5000 | On macOS the AirPlay Receiver holds :5000. That is why the API uses **5080** (`launchSettings.json`, `web/proxy.conf.json`). Start the API with `dotnet run --project src/Relay.Api` so the launch profile applies |
| The API stops at startup with `The connection string 'ConnectionStrings:Relay' is missing or empty. Copy .env.example to .env at the repository root …` | There is no `.env` in the repo root (or a parent folder of it), or its `ConnectionStrings__Relay` line is missing or empty. Run `cp .env.example .env` (Windows: `copy`) and set the password. This check runs only in Development; in Production the same problem shows up as a 500 on the first request |
| `Login failed for user 'sa'` right after changing the password, or the password in the connection string looks cut short | The password contains `$` and is not single-quoted in `.env`, so `${…}` expansion changed it. Quote it: `RELAY_DB_SA_PASSWORD='…'`. Also check that no `ConnectionStrings__Relay` environment variable is overriding `.env` |
| `Login failed for user 'sa'` | The volume was created with a different password. Put the old password back in `.env`, or reset with `docker compose down -v` (deletes the data) |
| `docker compose up` fails with `set RELAY_DB_SA_PASSWORD in .env` | `.env` is missing, or the variable is empty. Run `cp .env.example .env` and set it |
| The DB container exits or never turns healthy | The password is too weak for SQL Server; check `docker compose logs db`. On Apple Silicon, check that Rosetta emulation is enabled |
| Infrastructure/API tests fail with Docker or Testcontainers errors | Docker is not running. Start it, or run only `dotnet test --project tests/Relay.Core.Tests` |
| The first API start or first test run is slow | The SQL Server image is large and is pulled once, and under Rosetta it also starts more slowly. Seeding itself takes about 1 s |
| The dashboard shows "We couldn't load this week's activity. Try again." | The API is not running on 5080, or it can't reach the DB. Check terminal 1 |
