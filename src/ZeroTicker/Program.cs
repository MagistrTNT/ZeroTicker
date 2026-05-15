using System.Text.Json;
using ZeroTicker;

Console.WriteLine("ZeroTicker starting...");
await Worker.RunAsync(CancellationToken.None);
