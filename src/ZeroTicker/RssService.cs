namespace ZeroTicker;

public static class RssService
{
    private static readonly HttpClient Client = new();

    public static async Task<string> FetchAndFormatAsync(string url, CancellationToken ct)
    {
        var response = await Client.GetStringAsync(url, ct);
        // TODO: parse RSS via XDocument / XmlReader
        return response;
    }
}
