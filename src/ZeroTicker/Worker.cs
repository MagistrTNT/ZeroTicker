namespace ZeroTicker;

public static class Worker
{
    public static async Task RunAsync(CancellationToken ct)
    {
        Console.WriteLine("Worker stub");
        await Task.CompletedTask;
    }
}
