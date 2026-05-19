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

try
{
    var config = TickerConfig.Load(configPath);
    Console.WriteLine("ZeroTicker started");
    await Worker.RunAsync(config, configPath, CancellationToken.None);
}
catch (Exception ex)
{
    Console.WriteLine("Fatal: {0}", ex.Message);
}
