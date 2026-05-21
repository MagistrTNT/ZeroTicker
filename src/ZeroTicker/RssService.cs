using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace ZeroTicker;

public static class RssService
{
    private static readonly HttpClient Client = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    static RssService()
    {
        Client.DefaultRequestHeaders.UserAgent.ParseAdd("ZeroTicker/1.0");
    }

    private static readonly XNamespace DcNs = "http://purl.org/dc/elements/1.1/";

    private static DateTimeOffset? TryParsePubDate(XElement item, XNamespace ns)
    {
        var pubDateStr = item.Element(ns + "pubDate")?.Value;
        if (pubDateStr is not null && DateTimeOffset.TryParse(pubDateStr, out var d))
            return d;

        var dcDateStr = item.Element(DcNs + "date")?.Value;
        if (dcDateStr is not null && DateTimeOffset.TryParse(dcDateStr, out d))
            return d;

        return null;
    }

    public static async Task<List<(string Title, string Source)>> FetchAllAsync(
        string[] urls, int maxItems, string separator, int maxAgeMinutes, CancellationToken ct)
    {
        var headlines = new List<(string Title, string Source)>();
        var cutoff = maxAgeMinutes > 0 ? DateTimeOffset.UtcNow - TimeSpan.FromMinutes(maxAgeMinutes) : (DateTimeOffset?)null;

        foreach (var url in urls)
        {
            try
            {
                var xml = await Client.GetStringAsync(url, ct);
                var doc = XDocument.Parse(xml);
                var root = doc.Root;
                if (root is null) continue;
                var ns = root.Name.Namespace;
                var sourceName = doc.Descendants(ns + "channel")
                    .Elements(ns + "title").FirstOrDefault()?.Value ?? url;

                var firstTitle = "";
                var feedCount = 0;
                var ageSkipped = 0;
                foreach (var item in doc.Descendants(ns + "item"))
                {
                    if (feedCount >= maxItems) break;
                    var title = WebUtility.HtmlDecode(
                        item.Element(ns + "title")?.Value?.Trim());
                    if (string.IsNullOrEmpty(title)) continue;

                    if (cutoff is not null)
                    {
                        var pubDate = TryParsePubDate(item, ns);
                        if (pubDate is null || pubDate < cutoff)
                        {
                            ageSkipped++;
                            continue;
                        }
                    }

                    headlines.Add((title, sourceName));
                    if (feedCount == 0) firstTitle = title;
                    feedCount++;
                }

                var truncated = firstTitle.Length <= 80 ? firstTitle : firstTitle[..77] + "...";
                Console.WriteLine("[{0:HH:mm:ss}] {1}: {2} items", DateTime.Now, sourceName, feedCount);
                if (ageSkipped > 0)
                    Console.WriteLine("   ({0} filtered by age)", ageSkipped);
                Console.WriteLine("   {0} {1}", separator, truncated);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("[{0:HH:mm:ss}] [warn] Network error: {1} — {2}", DateTime.Now, url, ex.Message);
            }
            catch (XmlException ex)
            {
                Console.WriteLine("[{0:HH:mm:ss}] [warn] XML error: {1} — {2}", DateTime.Now, url, ex.Message);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("[{0:HH:mm:ss}] [warn] Timeout: {1}", DateTime.Now, url);
            }
        }

        return headlines;
    }
}
