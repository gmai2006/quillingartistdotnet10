using ApiService.Dao;

namespace ApiService.Services;

public class NightlyJob(IServiceScopeFactory scopeFactory, ILogger<NightlyJob> logger) : BackgroundService
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
    private static readonly TimeSpan RunAt = new(2, 0, 0);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = NextRunUtc(DateTimeOffset.UtcNow) - DateTimeOffset.UtcNow;
            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            await RunNightlyTaskAsync(stoppingToken);
        }
    }

    public async Task RunNightlyTaskAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Starting nightly job execution...");
        try
        {
            await PerformWorkAsync(ct);
            logger.LogInformation("Nightly job finished successfully.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Nightly job failed: {Message}", e.Message);
        }
    }

    private async Task PerformWorkAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var productsService = scope.ServiceProvider.GetRequiredService<ProductsService>();

        var rows = await productsService.SelectAllAsync(0, 1000, new ProductsFilter());

        var filtered = rows.Where(product => product.Stock < 3).ToList();
        logger.LogInformation("Nightly job found {Count} low-stock products", filtered.Count);
    }

    internal static DateTimeOffset NextRunUtc(DateTimeOffset nowUtc)
    {
        var local = TimeZoneInfo.ConvertTime(nowUtc, Zone);
        var candidate = local.Date + RunAt;
        if (candidate <= local.DateTime)
        {
            candidate = candidate.AddDays(1);
        }
        return new DateTimeOffset(candidate, Zone.GetUtcOffset(candidate)).ToUniversalTime();
    }
}
