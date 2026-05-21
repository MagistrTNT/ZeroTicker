using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZeroTicker;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(TickerConfig))]
[JsonSerializable(typeof(RssOutput))]
internal partial class TickerJsonContext : JsonSerializerContext { }

public record RssOutput
{
    [JsonPropertyName("text")]
    public string Text { get; init; } = "";
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; init; } = "";
    [JsonPropertyName("sources")]
    public string[] Sources { get; init; } = [];
}

public record RssSource
{
    public string Url { get; init; } = "";
    public string? Separator { get; init; }
}

public record TickerConfig
{
    public RssSource[] RssSources { get; init; } = [];
    public int IntervalMinutes { get; init; } = 10;
    public int MaxPerSource { get; init; } = 15;
    public int MaxAgeMinutes { get; init; } = 0;
    public string Separator { get; init; } = " | ";
    public string OutputPath { get; init; } = "news.js";

    public static TickerConfig Load(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Config not found: {fullPath}");
        var configDir = Path.GetDirectoryName(fullPath)!;
        var json = File.ReadAllText(fullPath);
        var config = JsonSerializer.Deserialize(json, TickerJsonContext.Default.TickerConfig)
            ?? throw new InvalidDataException("Config deserialization returned null");
        if (!Path.IsPathRooted(config.OutputPath))
            config = config with { OutputPath = Path.GetFullPath(config.OutputPath, configDir) };
        return config;
    }
}
