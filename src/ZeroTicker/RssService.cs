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

    public static async Task<List<(string Title, string Source)>> FetchAllAsync(
        string[] urls, int maxItems, CancellationToken ct)
    {
        var headlines = new List<(string Title, string Source)>();

        var perFeed = maxItems / urls.Length + 1;

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

                var feedCount = 0;
                var feedTitles = new List<string>();
                foreach (var item in doc.Descendants(ns + "item"))
                {
                    if (feedCount >= perFeed) break;
                    var title = WebUtility.HtmlDecode(
                        item.Element(ns + "title")?.Value?.Trim());
                    if (!string.IsNullOrEmpty(title))
                    {
                        headlines.Add((title, sourceName));
                        feedTitles.Add(title);
                        feedCount++;
                    }
                }

                Console.WriteLine("[{0:HH:mm:ss}] {1}: {2} items", DateTime.Now, sourceName, feedCount);
                foreach (var t in feedTitles)
                {
                    var truncated = t.Length <= 80 ? t : t[..77] + "...";
                    Console.WriteLine("  · {0}", truncated);
                }
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

        return headlines.Take(maxItems).ToList();
    }
}
