using Infrastructure.Querying;

namespace ImpactAPI.Tenders.Service;

public record TenderQueryParameters(
    int? Page,
    int? PageSize,
    DateTime? DateFrom,
    DateTime? DateTo,
    int? SupplierId,
    decimal? AwardedValueInEuroFrom,
    decimal? AwardedValueInEuroTo,
    TenderSortField? SortField,
    bool? SortDescending
) : PageRequest(Page, PageSize);

public enum TenderSortField
{
    Date,
    AwardedValueInEuro
}