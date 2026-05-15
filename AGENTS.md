# ZeroTicker — Agent Guide

## Repo state

- **Pre-implementation.** No code exists yet. Only `Идея.md` (Russian) as the spec.
- Empty git repo (no commits, no branches). First task: scaffold the project structure.

## Architecture

Two independent parts:

| Part | Stack | Entry |
|------|-------|-------|
| **Core** | C# .NET console app with Native AOT | `src/MagistrTicker.Core/Program.cs` |
| **View** | HTML/CSS/JS OBS widget | `obs-widget/index.html` |

Core generates `obs-widget/data.js` — View hot-reloads it via `<script>` tag replacement.

## Key technical decisions (from `Идея.md`)

- **.NET 10** SDK is installed (the plan says 8/9 — update `.csproj` target to `net10.0`).
- Native AOT: `<PublishAot>true</PublishAot>`, `<OptimizationPreference>Size</OptimizationPreference>`, `<TrimMode>full</TrimMode>`.
- `System.Text.Json` (not Newtonsoft), `System.ServiceModel.Syndication` for RSS.
- Single static `HttpClient` (or `IHttpClientFactory`).
- Atomic file writes: write to `.tmp` → rename to target.
- `PeriodicTimer` for worker loop (10 min default interval).
- `animation-duration` calculated in JS from text width so scroll speed is constant.

## Commands

```powershell
# Build + Publish (AOT, size-optimized)
dotnet publish src/MagistrTicker.Core -c Release

# Run (debug)
dotnet run --project src/MagistrTicker.Core
```

No test/lint infrastructure exists yet — add when needed.

## Conventions

- Core outputs to `obs-widget/data.js` by default (configurable via `appsettings.json`).
- Keep Core binary under 20 MB RAM — avoid heavy dependencies.
- Use `will-change: transform` and GPU-accelerated CSS for ticker scroll.
- View must work as an OBS Browser Source (no reload, seamless loop).
