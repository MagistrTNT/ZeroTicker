using System.Text.Json;

namespace ZeroTicker;

public static class Worker
{
    public static async Task RunAsync(TickerConfig config, CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(config.IntervalMinutes));

        Console.WriteLine("Worker started, interval: {0} min", config.IntervalMinutes);

        do
        {
            var now = DateTime.UtcNow;
            try
            {
                var items = await RssService.FetchAllAsync(config.RssUrls, config.MaxItems, ct);

                var output = new RssOutput
                {
                    Text = string.Join(config.Separator, items.Select(i => i.Title)),
                    Timestamp = now.ToString("O"),
                    Sources = items.Select(i => i.Source).Distinct().ToArray()
                };

                var js = "const rssData = " +
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
                Console.WriteLine("[{0:HH:mm:ss}] Worker error: {1}", DateTime.UtcNow, ex.Message);
            }
        }
        while (await timer.WaitForNextTickAsync(ct));
    }
}
