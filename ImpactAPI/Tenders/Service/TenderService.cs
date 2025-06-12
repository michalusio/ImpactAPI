using System.Globalization;
using System.Linq.Expressions;
using ImpactAPI.Tenders.Database;
using ImpactAPI.Tenders.External;
using Infrastructure.Extensions;
using Infrastructure.Querying;
using Microsoft.EntityFrameworkCore;

namespace ImpactAPI.Tenders.Service;

public class TenderService(TendersDbContext Database, TenderDownloaderHostedService DownloaderService)
{
    public TimeSpan TimeLeftToLoadAllTenders => DownloaderService.TimeLeft;

    public async Task<QueryResponse<TenderReadModel>> GetTenders(TenderQueryParameters queryParams, CancellationToken cancellationToken)
    {
        // The maximum page size is 100 - default to 100 when not provided
        var pageSize = Math.Min(queryParams.PageSize ?? 100, 100);

        Expression<Func<Tender, bool>> cursorCondition = queryParams.SortField switch
        {
            TenderSortField.Date => DateTime.TryParse(queryParams.PageAfter, CultureInfo.InvariantCulture, out var afterDate)
                ? (t) => t.Date > afterDate
                : (t) => false,
            TenderSortField.AwardedValueInEuro => decimal.TryParse(queryParams.PageAfter ?? string.Empty, CultureInfo.InvariantCulture, out var afterDecimal)
                ? (t) => t.AwardedValueInEuro > afterDecimal
                : (t) => false,
            _ => (t) => t.Id.CompareTo(queryParams.PageAfter!) > 0
        };

        Func<TenderReadModel, string> nextCursorGetter = queryParams.SortField switch
        {
            TenderSortField.Date => (t) => t.Date.ToString(),
            TenderSortField.AwardedValueInEuro => (t) => t.AwardedValueInEuro.ToString(),
            _ => (t) => t.Id
        };

        Expression<Func<Tender, object>> sortField = queryParams.SortField switch
        {
            TenderSortField.Date => (t) => t.Date,
            TenderSortField.AwardedValueInEuro => (t) => t.AwardedValueInEuro,
            _ => (t) => t.Id
        };

        var tendersQuery = Database.Tenders
            .OrderedBy(sortField, queryParams.SortDescending ?? false)
            .WhereIf(queryParams.SupplierId is not null, t => t.Suppliers.Any(s => s.Id == queryParams.SupplierId))
            .WhereIf(queryParams.DateFrom is not null, t => t.Date >= queryParams.DateFrom)
            .WhereIf(queryParams.DateTo is not null, t => t.Date <= queryParams.DateTo)
            .WhereIf(queryParams.AwardedValueInEuroFrom is not null, t => t.AwardedValueInEuro >= queryParams.AwardedValueInEuroFrom)
            .WhereIf(queryParams.AwardedValueInEuroTo is not null, t => t.AwardedValueInEuro <= queryParams.AwardedValueInEuroTo)
            .WhereIf(queryParams.PageAfter is not null, cursorCondition)
            .Select(TenderReadModel.MapFromTender);

        var tendersTotal = await tendersQuery.CountAsync(cancellationToken);
        var tenders = await tendersQuery
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new QueryResponse<TenderReadModel>(
            tenders,
            nextCursorGetter(tenders.Last()),
            pageSize,
            tendersTotal
        );
    }

    public async Task<TenderReadModel> GetTenderById(string id, CancellationToken cancellationToken)
    {
        var tender = await Database.Tenders
            .Where(t => t.Id == id)
            .Select(TenderReadModel.MapFromTender)
            .SingleOrDefaultAsync(cancellationToken);

        if (tender is null) throw new NotFoundException();

        return tender;
    }
}


