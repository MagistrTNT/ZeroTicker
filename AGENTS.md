# ZeroTicker — Agent Guide

## Repo state

- **Core implemented.** `src/ZeroTicker/` — C# .NET 10 Native AOT console app.
- **View implemented.** Flat-release structure: `index.html` + `style.css` + `script.js` at repo root.
- **Display settings** live in `config.js` (hand-edited), **not** in C#.
- Git branches: `main` (scaffold) + `dev` (active development) + feature branches.

## Architecture

| Part | Stack | Entry |
|------|-------|-------|
| **Core** | C# .NET console app with Native AOT | `src/ZeroTicker/Program.cs` |
| **View** | HTML/CSS/JS OBS widget | `index.html` |

```
appsettings.json ──→ Core ──→ news.js (только текст)
                                    │
config.js (ручное редактирование) ──┤
                                    ↓
                              script.js → OBS
```

Core generates `news.js` (no display config).  
View reads `config.js` (display settings) + `news.js` (text) independently.

## Key technical decisions

- **.NET 10**, Native AOT: `<PublishAot>true</PublishAot>`, `<OptimizationPreference>Size</OptimizationPreference>`, `<TrimMode>full</TrimMode>`.
- **RSS parsing** via `XDocument` (AOT-safe, avoids `System.ServiceModel.Syndication` trimming issues).
- **Config** via `System.Text.Json` source generators (AOT-safe).
- Single static `HttpClient` with 15s timeout.
- Atomic file writes: `.tmp` → rename, auto-creates parent directory.
- `PeriodicTimer` for worker loop (configurable interval, default 10 min).
- `FileSystemWatcher` on `appsettings.json` — Core re-reads config and regenerates `news.js` on change.
- Display config (`position`, `speed`, `fontSize`, `color`, etc.) is **only** in `config.js` — Core knows nothing about it.

## Files

| File | Responsibility |
|------|---------------|
| `Program.cs` | Entry point, config search (CWD → `src/ZeroTicker/` → exe dir), FileSystemWatcher |
| `RssService.cs` | `FetchAllAsync()` — fetches all URLs, `XDocument` RSS 2.0 parsing, per-feed try/catch, per-feed item limit |
| `Worker.cs` | `PeriodicTimer` loop, format → `RssOutput` → `FilePublisher`. Re-reads config on each tick |
| `FilePublisher.cs` | Atomic write with `Directory.CreateDirectory` |
| `TickerConfig.cs` | Config model + `Load()` with source-gen context. RSS-only — no ViewConfig |
| `config.js` | Display settings for the widget (hand-edited, not generated) |
| `index.html` | Minimal OBS Browser Source entry. Loads `config.js` then `script.js` |
| `style.css` | Base ticker styles. Configurable props set via JS inline |
| `script.js` | Seamless scroll loop via `requestAnimationFrame`, `Math.round` for crisp text, hot-reload of `news.js` every 60s, reads `window.tickerConfig` |

## Commands

```powershell
# Run (works from repo root or project dir)
dotnet run --project src/ZeroTicker

# Build + AOT publish (requires VS C++ workload)
dotnet publish src/ZeroTicker -c Release

# Run from project directory directly
cd src/ZeroTicker && dotnet run -c Release
```

No test/lint infrastructure exists yet — add when needed.

## Conventions

- Core outputs to `news.js` (configurable via `appsettings.json` as `OutputPath`). For dev from repo root uses `../../news.js`.
- Keep Core binary under 20 MB RAM — avoid heavy dependencies.
- `news.js` is in `.gitignore` — generated artifact. `config.js` is **not** gitignored — it's hand-edited.
- `config.js` changes take effect on OBS Browser Source refresh (no Core restart needed).
- Core config (`appsettings.json`) changes trigger `FileSystemWatcher` → Core immediately regenerates `news.js`.
- All changes go to `dev` branch — merge to `main` only for releases.

## Release deployment

`release/` is a portable drop-in folder. **Zero DLLs — single native AOT binary.**

### Structure

```
release/
  ZeroTicker.exe     (6-8 MB — native AOT, self-contained)
  appsettings.json   (OutputPath: "news.js" — flat path)
  config.js          (display settings — edit this for OBS look)
  index.html         (OBS Browser Source)
  style.css
  script.js
  news.js            (generated on first run, gitignored)
```

### How to update release after code changes

```powershell
dotnet publish src\ZeroTicker -c Release
Copy-Item src\ZeroTicker\bin\Release\net10.0\win-x64\publish\ZeroTicker.exe release\
```

Assumes `release\appsettings.json` has `"OutputPath": "news.js"` (flat, not `../../news.js` as used for dev from repo root).

### Prerequisites

Native AOT requires the **Desktop development with C++** workload in Visual Studio.  
Without it, `dotnet publish` fails with `Platform linker not found`.
