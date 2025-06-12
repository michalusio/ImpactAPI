using System.Diagnostics;
using System.Globalization;
using ImpactAPI.Tenders.Database;
using Microsoft.EntityFrameworkCore;

namespace ImpactAPI.Tenders.External;

public class TenderDownloaderHostedService(
    IServiceProvider ServiceProvider,
    ITendersGuruAPI TendersGuruApi,
    ILogger<TenderDownloaderHostedService> Logger
) : IHostedService, IDisposable
{
    /// <summary>
    /// Estimated time left to load all tenders (Zero if finished)
    /// </summary>
    public TimeSpan TimeLeft { get; private set; } = TimeSpan.MaxValue;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Setting off a long-running task
        var _ = Task.Run(async () =>
        {
            try
            {
                using var scope = ServiceProvider.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<TendersDbContext>();
                await Process(db, _cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                Logger.LogInformation("Stopped the service as requested");
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error while loading tenders");
            }
        }, cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    private async Task Process(TendersDbContext database, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Service started");
        var totalPagesWanted = 100;
        var pageSize = 100;
        var totalTendersWanted = totalPagesWanted * pageSize;

        var alreadyLoadedCount = await database.Tenders.CountAsync(cancellationToken);
        Logger.LogInformation("Previously downloaded tenders: {Tenders}", alreadyLoadedCount);

        var loadedSoFar = 0;
        var timeTakenWatch = Stopwatch.StartNew();
        while (alreadyLoadedCount + loadedSoFar < totalTendersWanted
            && !cancellationToken.IsCancellationRequested)
        {
            var pageData = await GetNextPages(alreadyLoadedCount + loadedSoFar, pageSize);
            await SaveToDatabase(database, pageData.SelectMany(p => p.Tenders), cancellationToken);

            loadedSoFar += pageData.Sum(p => p.Tenders.Count());
            RecalculateTimeLeft(alreadyLoadedCount, loadedSoFar, timeTakenWatch.Elapsed, totalTendersWanted);
            Logger.LogDebug("Loaded {TendersTotal} tenders total. Estimated time left: {TimeLeft}", alreadyLoadedCount + loadedSoFar, TimeLeft);
        }

        Logger.LogInformation("Downloaded all {Tenders} tenders", alreadyLoadedCount + loadedSoFar);
    }

    private async Task<IEnumerable<ITendersGuruAPI.TendersListDto>> GetNextPages(int loadedCount, int pageSize)
    {
        var nextPageIndex = 1 + loadedCount / pageSize;
        return await Task.WhenAll(
            TendersGuruApi.GetTenders(nextPageIndex),
            TendersGuruApi.GetTenders(nextPageIndex + 1)
        );
    }

    private static async Task SaveToDatabase(TendersDbContext database, IEnumerable<ITendersGuruAPI.TenderDto> tenders, CancellationToken cancellationToken)
    {
        var suppliersToAdd = tenders
            .SelectMany(t => t.Awards)
            .SelectMany(a => a.Suppliers)
            .DistinctBy(s => s.Id)
            .Select(s => new Supplier
            {
                Id = s.Id,
                Name = s.Name,
            })
            .ToDictionary(s => s.Id);

        var supplierIds = suppliersToAdd.Keys;

        var existingSuppliers = await database.Suppliers
            .Where(s => supplierIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        database.Suppliers.AddRange(suppliersToAdd.Values.Where(s => !existingSuppliers.ContainsKey(s.Id)));

        var tendersToAdd = tenders
            .Select(t => new Tender
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                AwardedValueInEuro = decimal.Parse(t.AwardedValueInEuro, CultureInfo.InvariantCulture),
                Date = t.Date,
                Suppliers = t.Awards
                    .SelectMany(a => a.Suppliers)
                    .DistinctBy(s => s.Id)
                    .Select(s => existingSuppliers.TryGetValue(s.Id, out var supplier)
                        ? supplier
                        : suppliersToAdd[s.Id])
                    .ToList()
            })
            .ToList();

        database.Tenders.AddRange(tendersToAdd);

        await database.SaveChangesAsync(cancellationToken);
    }

    private void RecalculateTimeLeft(int alreadyLoadedCount, int loadedSoFar, TimeSpan elapsed, int totalTendersWanted)
    {
        var neededToLoadYet = totalTendersWanted - alreadyLoadedCount - loadedSoFar;
        var timeSpentPerOneTender = elapsed / Math.Max(1, loadedSoFar);
        TimeLeft = timeSpentPerOneTender * neededToLoadYet;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _cancellationTokenSource.Dispose();
    }
}
