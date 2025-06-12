using System.Linq.Expressions;
using ImpactAPI.Tenders.Database;

namespace ImpactAPI.Tenders.Service;

public record TenderReadModel
(
    string Id,
    DateTime Date,
    string Title,
    string? Description,
    decimal AwardedValueInEuro,
    IEnumerable<SupplierReadModel> Suppliers
)
{
    public static readonly Expression<Func<Tender, TenderReadModel>> MapFromTender = t => new TenderReadModel(
        t.Id,
        t.Date,
        t.Title,
        t.Description,
        t.AwardedValueInEuro,
        t.Suppliers.Select(s => new SupplierReadModel(s.Id, s.Name))
    );
}

public record SupplierReadModel(
    int Id,
    string Name
);