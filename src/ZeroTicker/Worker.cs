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

                var (text, sources) = await RssService.FetchAllAsync(
                    config.RssSources, config.Separator, config.MaxPerSource, config.MaxAgeMinutes, ct);

                var output = new RssOutput
                {
                    Text = text,
                    Timestamp = now.ToString("O"),
                    Sources = sources
                };

                var js = "window.rssData = " +
                    JsonSerializer.Serialize(output, TickerJsonContext.Default.RssOutput) + ";";

                await FilePublisher.WriteAtomicallyAsync(config.OutputPath, js, ct);
                Console.WriteLine("[{0:HH:mm:ss}] Published {1} characters from {2} sources",
                    text.Length, sources.Length);
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
