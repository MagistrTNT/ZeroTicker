using System.Text.Json;

namespace ZeroTicker;

public static class Worker
{
    public static async Task RunAsync(TickerConfig config, string configPath, CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(config.IntervalMinutes));

        Console.WriteLine("[{0:HH:mm:ss}] Worker started, interval: {1} min", DateTime.Now, config.IntervalMinutes);

        do
        {
            var now = DateTime.Now;
            try
            {
                config = TickerConfig.Load(configPath);

                var items = await RssService.FetchAllAsync(config.RssUrls, config.MaxItems, ct);

                var output = new RssOutput
                {
                    Text = string.Concat(items.Select(i => config.Separator + i.Title)),
                    Timestamp = now.ToString("O"),
                    Sources = items.Select(i => i.Source).Distinct().ToArray()
                };

                var js = "window.rssData = " +
                    JsonSerializer.Serialize(output, TickerJsonContext.Default.RssOutput) + ";";

                await FilePublisher.WriteAtomicallyAsync(config.OutputPath, js, ct);
                Console.WriteLine("[{0:HH:mm:ss}] Published {1} headlines from {2} sources",
                    now, items.Count, output.Sources.Length);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[{0:HH:mm:ss}] Worker error: {1}", now, ex.Message);
            }
        }
        while (await timer.WaitForNextTickAsync(ct));
    }
}
