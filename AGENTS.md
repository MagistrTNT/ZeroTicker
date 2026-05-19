# ZeroTicker — Agent Guide

## Repo state

- **Core implemented.** `src/ZeroTicker/` — C# .NET 10 Native AOT console app.
- **View implemented.** Flat-release structure: `index.html` + `style.css` + `script.js` at repo root.
- Git branches: `main` (scaffold) + `dev` (active development).

## Architecture

| Part | Stack | Entry |
|------|-------|-------|
| **Core** | C# .NET console app with Native AOT | `src/ZeroTicker/Program.cs` |
| **View** | HTML/CSS/JS OBS widget | `index.html` |

Core generates `data.js` — View hot-reloads it via `<script>` tag replacement.

## Key technical decisions

- **.NET 10**, Native AOT: `<PublishAot>true</PublishAot>`, `<OptimizationPreference>Size</OptimizationPreference>`, `<TrimMode>full</TrimMode>`.
- **RSS parsing** via `XDocument` (AOT-safe, avoids `System.ServiceModel.Syndication` trimming issues).
- **Config** via `System.Text.Json` source generators (AOT-safe).
- Single static `HttpClient` with 15s timeout.
- Atomic file writes: `.tmp` → rename, auto-creates parent directory.
- `PeriodicTimer` for worker loop (configurable interval, default 10 min).
- View config (`position`, `speed`, `height`, `color`, etc.) is part of `appsettings.json` → passed through `data.js`.

## Files

| File | Responsibility |
|------|---------------|
| `Program.cs` | Entry point, config search (CWD → `src/ZeroTicker/` → exe dir) |
| `RssService.cs` | `FetchAllAsync()` — fetches all URLs, `XDocument` RSS 2.0 parsing, per-feed try/catch |
| `Worker.cs` | `PeriodicTimer` loop, format → `RssOutput` → `FilePublisher` |
| `FilePublisher.cs` | Atomic write with `Directory.CreateDirectory` |
| `TickerConfig.cs` | Config model + `Load()` with source-gen context. Includes `ViewConfig` |
| `index.html` | Minimal OBS Browser Source entry |
| `style.css` | Base ticker styles + `@keyframes ticker-scroll`. Configurable props set via JS inline |
| `script.js` | Seamless scroll loop, dynamic speed, hot-reload of `data.js` every 60s, reads `rssData.config` |

## Commands

```powershell
# Run (works from repo root or project dir)
dotnet run --project src/ZeroTicker

# Build + AOT publish
dotnet publish src/ZeroTicker -c Release

# Run from project directory directly
cd src/ZeroTicker && dotnet run -c Release
```

No test/lint infrastructure exists yet — add when needed.

## Conventions

- Core outputs to `data.js` (configurable via `appsettings.json` as `OutputPath`). For dev from repo root uses `../../data.js`.
- Keep Core binary under 20 MB RAM — avoid heavy dependencies.
- `data.js` is in `.gitignore` — generated artifact.
- All changes go to `dev` branch — merge to `main` only for releases.
