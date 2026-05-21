namespace ZeroTicker;

public static class FilePublisher
{
    public static async Task WriteAtomicallyAsync(string path, string content, CancellationToken ct)
    {
        var tmp = path + ".tmp";
        var dir = Path.GetDirectoryName(tmp);
        if (dir is not null) Directory.CreateDirectory(dir);
        await File.WriteAllTextAsync(tmp, content, ct);
        File.Move(tmp, path, overwrite: true);
    }
}
