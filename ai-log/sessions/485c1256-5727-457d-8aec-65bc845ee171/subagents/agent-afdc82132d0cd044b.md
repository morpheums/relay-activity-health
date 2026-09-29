
### 🧑 USER — 2026-09-28T23:51:22.428Z

Review this feature: OS-agnostic local configuration via DotNetEnv. The spec is PLAN.md §13, the entry "OS-agnostic local configuration via DotNetEnv".

**Setup:** in your worktree, run `git checkout --detach backend-infra-design-cleanup`, then `git diff main...HEAD`. There are 13 files.

**The commits:**
- `c1879c2` is a seed-test order fix. It has already been reviewed; skim it only.
- `3c3dce0`:
  - DotNetEnv 3.2.0 in `Directory.Packages.props`.
  - `Relay.Api.csproj` references DotNetEnv and `Microsoft.EntityFrameworkCore.Design` (PrivateAssets=all).
  - `Program.cs` calls `AddDotNetEnv` in Development only, with `TraversePath().NoClobber().NoEnvVars()`.
  - The migrator's fail-fast message now points at `.env`.
- `860fd64` deletes `RelayDesignTimeDbContextFactory`.
- `d2e12ae` drops EF Design from `Relay.Infrastructure` and updates `.claude/agents/backend.md`.
- The four `DotEnvConfigurationTests` and their fixtures were reviewed and approved as a red suite earlier. Now confirm they pass for the right reason.

**Check:**
- Correctness against PLAN §13.
- Is Production truly unaffected? That means no `.env` read and no change in configuration precedence.
- Do real environment variables still win over `.env`, and does the in-memory test configuration still win in `RelayApiFactory`?
- Is anything left that still needs `ConnectionStrings__Relay` exported in the shell? Grep `src`, `tests`, `docs`, `README.md`, `.env.example`, `docker-compose.yml`, `.claude/agents`, `CLAUDE.md`.
  - List README/docs hits separately, because product is about to rewrite those `ENV-SETUP` lines.
- Does `dotnet ef --startup-project src/Relay.Api` work without the factory?
- CLAUDE.md code rules (no comment blocks, naming), scope, and no secrets.

**Run:**
- `dotnet build`
- `dotnet test` over the full solution. Docker is available; the expected result is 402 passing.

Report ranked findings, split into (a) objective defects, with file:line and a suggested owner, and (b) items needing a user or design decision. Use the CLAUDE.md handoff format and end with a verdict of APPROVED or CHANGES REQUESTED.


**tool call — Bash**
```json
{"command":"git checkout --detach backend-infra-design-cleanup 2>&1 | tail -1; git diff main...HEAD --stat; git diff main...HEAD -- . ':!tests/Relay.Infrastructure.Tests' | head -400"}
```


**tool call — Bash**
```json
{"command":"grep -n -B2 -A25 \"OS-agnostic local configuration\" PLAN.md"}
```


<details><summary>tool result</summary>

```
HEAD is now at d2e12ae Drop EF Core Design package from Relay.Infrastructure; EF tools use Relay.Api as startup project
 .claude/agents/backend.md                          |  4 +-
 Directory.Packages.props                           |  1 +
 .../Composition/DevelopmentDatabaseMigrator.cs     |  2 +-
 src/Relay.Api/Program.cs                           |  9 +++
 src/Relay.Api/Relay.Api.csproj                     |  5 ++
 .../Persistence/RelayDesignTimeDbContextFactory.cs | 23 ------
 .../Relay.Infrastructure.csproj                    |  4 --
 tests/Relay.Api.Tests/Fixtures/DotEnvApiFactory.cs | 23 ++++++
 tests/Relay.Api.Tests/Fixtures/DotEnvDirectory.cs  | 25 +++++++
 .../Fixtures/EnvironmentVariableScope.cs           | 23 ++++++
 .../Fixtures/ProcessEnvironmentTestGroup.cs        |  7 ++
 tests/Relay.Api.Tests/Seed/SeedLoadTests.cs        | 17 ++---
 .../Startup/DotEnvConfigurationTests.cs            | 81 ++++++++++++++++++++++
 13 files changed, 186 insertions(+), 38 deletions(-)
diff --git a/.claude/agents/backend.md b/.claude/agents/backend.md
index 8778a96..43e5adf 100644
--- a/.claude/agents/backend.md
+++ b/.claude/agents/backend.md
@@ -16,8 +16,8 @@ Structure only; no contracts, no logic (the architect adds contracts afterwards)
 - `Relay.sln` with `src/Relay.Core`, `src/Relay.Infrastructure`, `src/Relay.Api`, `tests/Relay.Core.Tests`, `tests/Relay.Infrastructure.Tests`, `tests/Relay.Api.Tests`.
 - `Directory.Build.props`: `net10.0`, `Nullable=enable`, `ImplicitUsings=en …[clipped in markdown; full text in raw/]
```
</details>


<details><summary>tool result</summary>

```
560-- **README:** `docker compose up -d db && cd web && npx playwright install chromium && npm run e2e` (with `ConnectionStrings__Relay` set).
561-
562:### 2026-09-28 — OS-agnostic local configuration via DotNetEnv (user decision, validated by the architect)
563-
564-**Reason.** The run steps required `set -a; source .env; set +a` (bash/zsh only). User: *"We must find an env agnostic solution"* and *"Avoid hand written as much as possible;
565-if there is already a library for that, do not reinvent the wheel"* (the architect's first proposal was a hand-written reader).
566-- In Development only, the API adds the repo-root `.env` as a configuration source with **DotNetEnv 3.2.0** (`AddDotNetEnv(…, LoadOptions.TraversePath().NoClobber().NoEnvVars())`).
567-  `.env` is unchanged: its `ConnectionStrings__Relay` line is interpolated from `RELAY_DB_PORT` / `RELAY_DB_SA_PASSWORD`, the same file Docker Compose reads. Real environment variables and
568-  test configuration still win; the process environment is not modified; Production never reads `.env`.
569-- `RelayDesignTimeDbContextFactory` is removed; `dotnet ef … --startup-project src/Relay.Api` uses the same configuration (Api references `Microsoft.EntityFrameworkCore.Design`, private).
570-- Run steps are identical on Windows, macOS and Linux: `cp .env.example .env` → set the password → `docker compose up -d --wait db` → `dotnet run --project src/Relay.Api` →
571-  `cd web && npm start`. A password containing `$` must be single …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cat .env.example; cat .gitignore | grep -n env; grep -rn \"ConnectionStrings__Relay\\|source .env\\|set -a\" src tests docs README.md .env.example docker-compose.yml .claude CLAUDE.md scripts PLAN.md web/package.json web/playwright* web/e2e 2>/dev/null | grep -v node_modules"}
```


<details><summary>tool result</summary>

```
RELAY_DB_SA_PASSWORD="<redacted>"
RELAY_DB_PORT="1433"
ConnectionStrings__Relay="Server=localhost,${RELAY_DB_PORT};Database=relay;User Id=sa;Password=${RELAY_DB_SA_PASSWORD};TrustServerCertificate=True"
2:.env
(eval):1: no matches found: web/playwright*
```
</details>


**tool call — Bash**
```json
{"command":"(dotnet build 2>&1 | tail -5; dotnet test 2>&1 | tail -15)","timeout":600000}
```


<details><summary>tool result</summary>

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.75
Running tests from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-afdc82132d0cd044b/tests/Relay.Api.Tests/bin/Debug/net10.0/Relay.Api.Tests.dll (net10.0|arm64)
Running tests from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-afdc82132d0cd044b/tests/Relay.Infrastructure.Tests/bin/Debug/net10.0/Relay.Infrastructure.Tests.dll (net10.0|arm64)
/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-afdc82132d0cd044b/tests/Relay.Infrastructure.Tests/bin/Debug/net10.0/Relay.Infrastructure.Tests.dll (net10.0|arm64) passed (14s 908ms)
/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-afdc82132d0cd044b/tests/Relay.Api.Tests/bin/Debug/net10.0/Relay.Api.Tests.dll (net10.0|arm64) passed (20s 408ms)

Test run summary: Passed!
  /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-afdc82132d0cd044b/tests/Relay.Infrastructure.Tests/bin/Debug/net10.0/Relay.Infrastructure.Tests.dll (net10.0|arm64) passed (14s 908ms)
  /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-afdc82132d0cd044b/tests/Relay.Core.Tests/bin/Debug/net10.0/Relay.Core.Tests.dll (net10.0|arm64) passed (255ms)
  /Users/josemejia/Documents …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"grep -rnI \"ConnectionStrings__Relay\\|source .env\\|set -a\\|ConnectionStrings:Relay\" src tests docs README.md .env.example docker-compose.yml .claude CLAUDE.md scripts PLAN.md web/package.json web/e2e web/playwright.config.ts 2>/dev/null | grep -v node_modules | cut -c1-230; ls; ls src/Relay.Api; git status --short | head"}
```


<details><summary>tool result</summary>

```
tests/Relay.Api.Tests/Fixtures/DotEnvDirectory.cs:13:        ConnectionStrings__Relay="Server=localhost,${RELAY_DB_PORT};Database=relay;User Id=sa;Password=${RELAY_DB_SA_PASSWORD};TrustServerCertificate=True"
tests/Relay.Api.Tests/Startup/DotEnvConfigurationTests.cs:11:    private const string ConnectionStringVariable = "ConnectionStrings__Relay";
tests/Relay.Api.Tests/Startup/ConnectionStringTests.cs:29:            message => message.Contains("ConnectionStrings:Relay", StringComparison.Ordinal)
tests/Relay.Api.Tests/Startup/ConnectionStringTests.cs:30:                || message.Contains("ConnectionStrings__Relay", StringComparison.Ordinal));
.claude/agents/database.md:20:- Connection string key `ConnectionStrings:Relay`, supplied **only** by the environment (`ConnectionStrings__Relay`) — never committed (PLAN §13).
.env.example:3:ConnectionStrings__Relay="Server=localhost,${RELAY_DB_PORT};Database=relay;User Id=sa;Password=${RELAY_DB_SA_PASSWORD};TrustServerCertificate=True"
PLAN.md:516:  the API connection string comes only from the environment (`ConnectionStrings__Relay`), not `appsettings.Development.json`. Supersedes §5.1's "connection string in
PLAN.md:557:- **Running:** Playwright `webServer` starts the API (`dotnet run --project ../src/Relay.Api`, `ConnectionStrings__Relay` from the environment) and `ng serve`, reusing running servers;
PLAN.md:560:- **README:** `docker compose up -d db && cd web && npx playwright install chromium && npm run e2e` (with `ConnectionStrin …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"grep -rniI \"ENV-SETUP\\|\\.env\\b\\|export \\|environment variable\" README.md docs docker-compose.yml scripts web/package.json web/e2e web/*.ts 2>/dev/null | grep -v node_modules | cut -c1-250; git status --short; ls TestResults | head -3; git check-ignore TestResults; cat src/Relay.Api/Properties/launchSettings.json src/Relay.Api/appsettings.Development.json"}
```


<details><summary>tool result</summary>

```
(eval):1: no matches found: web/*.ts
TestResults
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5080",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```
</details>


**tool call — Bash**
```json
{"command":"grep -rniI \"ENV-SETUP\\|\\.env\\b\\|export \\|environment variable\\|ConnectionStrings\" README.md docs docker-compose.yml scripts web/package.json web/e2e 2>/dev/null | cut -c1-250; ls web | head -30; grep -rn \"webServer\" -A8 web/playwright.config.* 2>/dev/null | head -20"}
```


<details><summary>tool result</summary>

```
docker-compose.yml:10:      MSSQL_SA_PASSWORD: <redacted> RELAY_DB_SA_PASSWORD <redacted> .env}
angular.json
package-lock.json
package.json
proxy.conf.json
src
tsconfig.app.json
tsconfig.json
tsconfig.spec.json
(eval):1: no matches found: web/playwright.config.*
```
</details>


**tool call — Bash**
```json
{"command":"S=/private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad; rm -rf $S/p; mkdir -p $S/p/a/b/c && cd $S/p/a/b/c && printf 'FOO_X=\"from-parent\"\\nConnectionStrings__Relay=\"x-${FOO_X}\"\\n' > $S/p/a/.env\ncat > p.csproj <<'E'\n<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings></PropertyGroup>\n<ItemGroup><PackageReference Include=\"DotNetEnv\" Version=\"3.2.0\"/><PackageReference Include=\"Microsoft.Extensions.Configuration.CommandLine\" Version=\"10.0.0\"/><PackageReference Include=\"Microsoft.Extensions.Configuration.EnvironmentVariables\" Version=\"10.0.0\"/></ItemGroup></Project>\nE\ncat > Program.cs <<'E'\nusing DotNetEnv; using DotNetEnv.Configuration; using Microsoft.Extensions.Configuration;\nvar b = new ConfigurationBuilder().AddEnvironmentVariables().AddCommandLine(args);\nb.AddDotNetEnv(Path.Combine(Directory.GetCurrentDirectory(), \".env\"), LoadOptions.TraversePath().NoClobber().NoEnvVars());\nvar c = b.Build();\nConsole.WriteLine($\"FOO_X={c[\"FOO_X\"]} CS={c[\"ConnectionStrings:Relay\"]}\");\nE\ndotnet run 2>&1 | tail -3; echo ---; dotnet run -- --ConnectionStrings:Relay=cli 2>&1 | tail -2; echo ---; ConnectionStrings__Relay=env FOO_X=envfoo dotnet run 2>&1 | tail -2","timeout":300000}
```


<details><summary>tool result</summary>

```
FOO_X=from-parent CS=x-from-parent
---
FOO_X=from-parent CS=x-from-parent
---
FOO_X=envfoo CS=env
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-afdc82132d0cd044b\"; dotnet ef --version 2>&1 | tail -2; ConnectionStrings__Relay=\"Server=localhost,1433;Database=relay;User Id=sa;Password=x;TrustServerCertificate=True\" dotnet ef migrations list --startup-project src/Relay.Api --project src/Relay.Infrastructure --no-build 2>&1 | tail -8; echo ---; env -u ConnectionStrings__Relay ASPNETCORE_ENVIRONMENT=Development dotnet ef migrations list --startup-project src/Relay.Api --project src/Relay.Infrastructure --no-build 2>&1 | tail -6; grep -rn \"IDesignTime\\|Design\" src --include=*.cs --include=*.csproj | grep -v /obj/ | head; grep -n \"Design\" -B2 -A3 src/Relay.Infrastructure/Composition/*.cs | head -30","timeout":300000}
```


<details><summary>tool result</summary>

```
Entity Framework Core .NET Command-line Tools
10.0.5
The Entity Framework tools version '10.0.5' is older than that of the runtime '10.0.12'. Update the tools for the latest features and bug fixes. See https://aka.ms/AAc1fbw for more information.
An error occurred while accessing the database. Continuing without the information provided by the database. Error: Login failed for user 'sa'.
20260928203636_InitialCreate
20260928203718_LoadSeedData
Pending status not shown. Unable to determine which migrations have been applied. This can happen when your project uses a version of Entity Framework Core lower than 5.0.0 or when an error occurs while accessing the database.
---
The Entity Framework tools version '10.0.5' is older than that of the runtime '10.0.12'. Update the tools for the latest features and bug fixes. See https://aka.ms/AAc1fbw for more information.
An error occurred while accessing the database. Continuing without the information provided by the database. Error: The ConnectionString property has not been initialized.
20260928203636_InitialCreate
20260928203718_LoadSeedData
Pending status not shown. Unable to determine which migrations have been applied. This can happen when your project uses a version of Entity Framework Core lower than 5.0.0 or when an error occurs while accessing the database.
(eval):1: no matches found: --include=*.cs
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-afdc82132d0cd044b\"; ls -a ../../../.env 2>&1; cat tests/Relay.Api.Tests/Fixtures/RelayApiFactory.cs | head -50; git show c1879c2 --stat | tail -3; grep -rn \"//\\|/\\*\" src/Relay.Api/Program.cs tests/Relay.Api.Tests/Fixtures/DotEnv* tests/Relay.Api.Tests/Fixtures/EnvironmentVariableScope.cs tests/Relay.Api.Tests/Startup/DotEnvConfigurationTests.cs; grep -n \"Design\" src/*/*.csproj Directory.Packages.props; grep -rn \"ConnectionStringEnvironmentVariable\" src | head"}
```


<details><summary>tool result</summary>

```
ls: ../../../.env: No such file or directory
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Relay.Infrastructure.Composition;

namespace Relay.Api.Tests.Fixtures;

public sealed class RelayApiFactory(
    string? connectionString,
    string environmentName,
    Action<IServiceCollection>? overrideServices = null) : WebApplicationFactory<Program>
{
    public const string ConnectionStringSetting = $"ConnectionStrings:{InfrastructureServiceCollectionExtensions.ConnectionStringName}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environmentName);
        builder.ConfigureAppConfiguration(configuration =>
            configuration.AddInMemoryCollection([new KeyValuePair<string, string?>(ConnectionStringSetting, connectionString)]));
        if (overrideServices is not null)
        {
            builder.ConfigureTestServices(overrideServices);
        }
    }
}

 tests/Relay.Api.Tests/Seed/SeedLoadTests.cs | 17 +++++++++--------
 1 file changed, 9 insertions(+), 8 deletions(-)
Directory.Packages.props:7:    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.12" />
src/Relay.Api/Relay.Api.csproj:5:    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" PrivateAssets="all" />
src/Relay.Api/Composition/DevelopmentDatabaseM …[clipped in markdown; full text in raw/]
```
</details>


### 🤖 ASSISTANT — 2026-09-28T23:53:58.307Z

## Verdict: APPROVED

Approved with two minor documentation fixes. There are no objective defects in code or tests. `dotnet build` is clean (0 warnings, 0 errors). `dotnet test` over the full solution gives 402 passed, 0 failed, 0 skipped.

## Summary
The feature matches PLAN §13. `.env` is loaded only in Development, real environment variables win, and the process environment is not modified. The four `DotEnvConfigurationTests` pass for the right reason. `dotnet ef --startup-project src/Relay.Api` works without the factory. Two stale docs and one precedence quirk remain.

## Changed
Nothing edited by me. I reviewed the 13 files in `main...HEAD` and left no files in the repo. My probe project is in the scratchpad.

## Verification
- **Build and tests:**
  - `dotnet build` gave 0 warnings and 0 errors.
  - `dotnet test` gave total 402, failed 0, succeeded 402, skipped 0.
- **Real DotNetEnv 3.2.0 behaviour**, using a throwaway console project with the same `AddDotNetEnv(..., TraversePath().NoClobber().NoEnvVars())` call. The `.env` sat three directories above the working directory, with env vars and command-line sources registered before it, as in `WebApplication.CreateBuilder`:
  - `.env` only: `CS=x-from-parent`. Traversal upward works and `${FOO_X}` interpolation works.
  - `--ConnectionStrings:Relay=cli` on the command line: still `x-from-parent`. The `.env` value beat the CLI argument.
  - Env vars `ConnectionStrings__Relay=env` and `FOO_X=envfoo`: `CS=env`, `FOO_X=envfoo`. The real environment wins.
- **EF tools:** `dotnet ef migrations list --startup-project src/Relay.Api --project src/Relay.Infrastructure --no-build` lists `InitialCreate` and `LoadSeedData` with no factory.
  - With `ConnectionStrings__Relay` set, it reaches the database and fails on login. That is the expected outcome of my fake password.
  - With the variable unset, it reports "ConnectionString property has not been initialized". That is expected with no `.env` present.
  - I did not verify `.env` loading through `dotnet ef`. I did not want to create a `.env` in the repo.

## Checks against your list
- **PLAN §13:** clean. `Program.cs` matches the entry exactly. The `.env` path is `ContentRootPath/.env`, and `TraversePath` finds the repo-root file from `src/Relay.Api`, confirmed by my probe. The Design package is now private on Api only, and Infrastructure no longer has it.
- **Production:**
  - `Program.cs` guards `AddDotNetEnv` with `IsDevelopment()`, so no `.env` is read and no source is added. Precedence is unchanged.
  - `StartInProductionIgnoresTheDotEnvThatDevelopmentLoads` covers it. It compares against a Development factory over the same content root, so it is not vacuous.
  - DotNetEnv is still referenced by the Production binary but never invoked.
- **Real env var over `.env`:** proven by test 2, by the ordering, and by my probe.
- **`RelayApiFactory` in-memory configuration:** still wins.
  - The `ConfigureAppConfiguration` in-memory source is added after `Program.cs` runs `AddDotNetEnv`.
  - A key present with a null value in a later source still overrides, so the fail-fast test stays valid even if a developer has a `.env`.
  - No test pins this, because there is no `.env` in the test content root. I judge that acceptable.
- **Do the four tests pass for the right reason?** Yes.
  - The `.env` content is set in the temp content root.
  - The `EnvironmentVariableScope` in the test class constructor clears the three variables, so the `.env` provider is the only possible source.
  - The expected string in the fixture is hand-derived from `DotEnvContent`, not copied from output.
  - The collection has `DisableParallelization`, so the process-env mutation is safe.
  - The migrator is removed in `DotEnvApiFactory`, so no database is needed.
- **Grep for `ConnectionStrings__Relay` needed in the shell:** nothing in `src` requires it; the only source hit is the migrator message, which now points at `.env` first.
  - **`tests`:** only the new tests and the fail-fast assertion.
  - **`.claude/agents`, `CLAUDE.md`, `docker-compose.yml`:** see the stale-doc finding below. `docker-compose.yml:10` reads `RELAY_DB_SA_PASSWORD` from `.env`, which is fine.
  - **README/docs hits (for product's ENV-SETUP rewrite):** none. `README.md` and `docs/` contain no `.env`, `export`, `ConnectionStrings` or `ENV-SETUP` text, and `web/e2e` does not exist yet.
  - The only shell-export references are in `PLAN.md`, listed below.
- **Code rules:**
  - No comments in `Program.cs` or the new test and fixture files.
  - Names are descriptive.
  - Nothing outside scope.
  - No secrets: `.env` is gitignored, and `.env.example` and the test password are placeholders.
  - `SeedLoadTests` only changes ordering (`ignoreOrder: true`); it is already reviewed.
- **Comment/dead code:** the deleted factory has no remaining references. The only remaining `ConnectionStringEnvironmentVariable` use is the migrator message.

## (a) Objective defects
Only two low-severity documentation drifts, in files the backend agent should not touch.
1. [Minor, owner: architect/product] `PLAN.md:557` and `PLAN.md:560` still say "with `ConnectionStrings__Relay` set" and "from the environment". `PLAN.md:516` says the connection string comes "only from the environment, not `appsettings.Development.json`". These now conflict with §13 line 566, which reads `.env` in Development. They are planned Playwright/README text, so they should be updated together with the ENV-SETUP rewrite.
2. [Minor, owner: architect for `.claude/agents/database.md:20`] It says the connection string is supplied "only by the environment (`ConnectionStrings__Relay`)". It should mention Development `.env`.

## (b) Items needing a user or design decision
1. **Command-line argument precedence.**
   - I verified that in Development a `.env` value overrides `--ConnectionStrings:Relay=…` on the command line, because the `.env` source is added after the CLI source. Real env vars are not affected.
   - The .NET convention is that CLI args win. PLAN §13 only promises that env vars and test configuration win, so this is not a defect.
   - Decide whether to accept it (Development-only, low impact) or document it.
2. **`TraversePath` is not exercised by the tests.** The fixtures put `.env` directly in the content root, while the real layout has it two directories up from `src/Relay.Api`. I proved traversal in the probe. Adding a nested content-root test would guard against a future DotNetEnv change. This is optional.
3. **Quoting.** A `$` in the password must be single-quoted (PLAN §13 states it), but `.env.example` uses a double-quoted value, so it is not the safe form for a password with `$`. Consider a one-line note in the README rewrite.

## Next
Product rewrites the README ENV-SETUP text. The PLAN §13 lines 516, 557 and 560 and `database.md:20` need aligning at the same time.

## Deviations
None found against PLAN §13.

## Uncertain
- `dotnet ef` loading `.env` end to end, as described under Verification.
- The EF tools version 10.0.5 is older than the 10.0.12 runtime. The warning is cosmetic and unrelated to this change.

