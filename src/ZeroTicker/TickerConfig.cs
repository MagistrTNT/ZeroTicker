using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZeroTicker;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(TickerConfig))]
[JsonSerializable(typeof(RssOutput))]
[JsonSerializable(typeof(ViewConfig))]
internal partial class TickerJsonContext : JsonSerializerContext { }

public record ViewConfig
{
    [JsonPropertyName("position")]
    public string Position { get; init; } = "top";
    [JsonPropertyName("speed")]
    public int Speed { get; init; } = 60;
    [JsonPropertyName("height")]
    public int Height { get; init; } = 40;
    [JsonPropertyName("fontSize")]
    public int FontSize { get; init; } = 16;
    [JsonPropertyName("color")]
    public string Color { get; init; } = "#ffd700";
    [JsonPropertyName("background")]
    public string Background { get; init; } = "rgba(0,0,0,0.88)";
}

public record RssOutput
{
    [JsonPropertyName("text")]
    public string Text { get; init; } = "";
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; init; } = "";
    [JsonPropertyName("sources")]
    public string[] Sources { get; init; } = [];
    [JsonPropertyName("config")]
    public ViewConfig WidgetConfig { get; init; } = new();
}

public record TickerConfig
{
    public string[] RssUrls { get; init; } = [];
    public int IntervalMinutes { get; init; } = 10;
    public int MaxItems { get; init; } = 30;
    public string Separator { get; init; } = " ⛤ ";
    public string OutputPath { get; init; } = "data.js";
    public ViewConfig? ViewConfig { get; init; }

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
