using ZeroTicker;

try
{
    var config = TickerConfig.Load("appsettings.json");
    Console.WriteLine("ZeroTicker started");
    await Worker.RunAsync(config, CancellationToken.None);
}
catch (Exception ex)
{
    Console.WriteLine("Fatal: {0}", ex.Message);
}
