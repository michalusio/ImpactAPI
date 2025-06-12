using Infrastructure.Querying;

namespace ImpactAPI.Tenders.Service;

public record TenderQueryParameters(
    string? PageAfter,
    int? PageSize,
    DateTime? DateFrom,
    DateTime? DateTo,
    int? SupplierId,
    decimal? AwardedValueInEuroFrom,
    decimal? AwardedValueInEuroTo,
    TenderSortField? SortField,
    bool? SortDescending
) : QueryRequest(PageAfter, PageSize);

public enum TenderSortField
{
    Date,
    AwardedValueInEuro
}