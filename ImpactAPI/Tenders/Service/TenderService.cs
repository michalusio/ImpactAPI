using System.Linq.Expressions;
using ImpactAPI.Tenders.Database;
using Infrastructure.Extensions;
using Infrastructure.Querying;
using Microsoft.EntityFrameworkCore;

namespace ImpactAPI.Tenders.Service;

public class TenderService(TendersDbContext Database)
{
    public async Task<QueryResponse<TenderReadModel>> GetTenders(TenderQueryParameters queryParams, CancellationToken cancellationToken)
    {
        var page = Math.Max(queryParams.Page ?? 1, 1);

        // The maximum page size is 100 - default to 100 when not provided
        var pageSize = Math.Min(queryParams.PageSize ?? 100, 100);

        var sortDescending = queryParams.SortDescending ?? false;

        Expression<Func<Tender, object>> sortField = queryParams.SortField switch
        {
            TenderSortField.Date => (t) => t.Date,
            TenderSortField.AwardedValueInEuro => (t) => t.AwardedValueInEuro,
            _ => (t) => t.Id
        };

        var tendersQuery = Database.Tenders
            .OrderedBy(sortField, sortDescending)
            .WhereIf(queryParams.SupplierId is not null, t => t.Suppliers.Any(s => s.Id == queryParams.SupplierId))
            .WhereIf(queryParams.DateFrom is not null, t => t.Date >= queryParams.DateFrom)
            .WhereIf(queryParams.DateTo is not null, t => t.Date <= queryParams.DateTo)
            .WhereIf(queryParams.AwardedValueInEuroFrom is not null, t => t.AwardedValueInEuro >= queryParams.AwardedValueInEuroFrom)
            .WhereIf(queryParams.AwardedValueInEuroTo is not null, t => t.AwardedValueInEuro <= queryParams.AwardedValueInEuroTo)
            .Select(TenderReadModel.MapFromTender);

        var tendersTotal = await tendersQuery.CountAsync(cancellationToken);
        var tenders = await tendersQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new QueryResponse<TenderReadModel>(
            tenders,
            page,
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


