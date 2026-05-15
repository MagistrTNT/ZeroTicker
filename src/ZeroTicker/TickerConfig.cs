namespace ZeroTicker;

public record TickerConfig
{
    public string[] RssUrls { get; init; } = [];
    public int IntervalMinutes { get; init; } = 10;
    public string OutputPath { get; init; } = "obs-widget/data.js";
}
