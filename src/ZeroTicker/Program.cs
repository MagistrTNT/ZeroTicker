using System.IO;
using ZeroTicker;

var configPaths = new[]
{
    "appsettings.json",
    Path.Combine("src", "ZeroTicker", "appsettings.json"),
    Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
};

string? configPath = configPaths.FirstOrDefault(File.Exists);
if (configPath is null)
{
    Console.WriteLine("Fatal: appsettings.json not found (tried: {0})",
        string.Join(", ", configPaths));
    return;
}

Console.WriteLine("[{0:HH:mm:ss}] ZeroTicker started (config: {1})", DateTime.Now, configPath);

var watcherCts = new CancellationTokenSource();
configPath = Path.GetFullPath(configPath);
var configDir = Path.GetDirectoryName(configPath) ?? ".";

var lastChange = DateTime.MinValue;

using var watcher = new FileSystemWatcher(configDir, "appsettings.json")
{
    EnableRaisingEvents = true,
    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
};

watcher.Changed += ConfigChanged;
watcher.Created += ConfigChanged;
watcher.Renamed += ConfigChanged;

void ConfigChanged(object s, FileSystemEventArgs e)
{
    var now = DateTime.Now;
    if ((now - lastChange).TotalMilliseconds < 500) return;
    lastChange = now;
    Console.WriteLine("[{0:HH:mm:ss}] [config] appsettings.json changed, restarting worker...", now);
    watcherCts.Cancel();
}

while (true)
{
    try
    {
        var config = TickerConfig.Load(configPath);
        await Worker.RunAsync(config, configPath, watcherCts.Token);
        break;
    }
    catch (OperationCanceledException) when (!watcherCts.IsCancellationRequested)
    {
        // worker exited for unknown reason — don't restart
        break;
    }
    catch (OperationCanceledException)
    {
        watcherCts = new CancellationTokenSource();
    }
}