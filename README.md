# ZeroTicker

Lightweight RSS ticker for OBS. Two parts:

- **Core** — C# .NET 10 Native AOT console app. Fetches RSS feeds, formats headlines, writes to `obs-widget/data.js`.
- **View** — HTML/CSS/JS widget for OBS Browser Source. Infinite scroll with GPU-accelerated CSS, hot-reload via `<script>` replacement.

## Build

```powershell
dotnet publish src/ZeroTicker -c Release
```

Output: `src/ZeroTicker/bin/Release/net10.0/win-x64/publish/ZeroTicker.exe`

## Run (debug)

```powershell
dotnet run --project src/ZeroTicker
```

## License

GPL-3.0
