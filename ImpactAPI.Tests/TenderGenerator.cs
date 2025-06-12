using ImpactAPI.Tenders.Database;

namespace ImpactAPI.Tests;
internal static class TenderGenerator
{
    private static readonly Dictionary<string, Tender> TenderCache = [];
    private static readonly Dictionary<int, Supplier> SupplierCache = [];

    public static Tender RandomNonDuplicateTender
    {
        get
        {
            var id = "";
            do
            {
                id = RandomTenderId;
            }
            while (TenderCache.ContainsKey(id));

            var tender = new Tender()
            {
                Id = id,
                AwardedValueInEuro = ((decimal)Random.Shared.NextDouble()) * 1000m,
                Date = DateTime.Now.AddHours(-Random.Shared.Next(0, 1000)),
                Description = "No interesting description",
                Title = "A test tender",
                Suppliers = Enumerable.Range(0, Random.Shared.Next(1, 3))
                    .Select(_ => RandomSupplier)
                    .ToList()
            };
            TenderCache[id] = tender;
            return tender;
        }
    }

    public static Tender RandomTender
    {
        get
        {
            var id = RandomTenderId;
            if (TenderCache.TryGetValue(id, out var tender))
            {
                tender.Suppliers = [];
                return tender;
            }
            tender = new()
            {
                Id = id,
                AwardedValueInEuro = ((decimal)Random.Shared.NextDouble()) * 1000m,
                Date = DateTime.Now.AddHours(-Random.Shared.Next(0, 1000)),
                Description = "No interesting description",
                Title = "A test tender",
                Suppliers = Enumerable.Range(0, Random.Shared.Next(1, 3))
                    .Select(_ => RandomSupplier)
                    .ToList()
            };
            TenderCache[id] = tender;
            return tender;
        }
    }

    private static Supplier RandomSupplier
    {
        get
        {
            var id = Random.Shared.Next(1, 10);
            if (SupplierCache.TryGetValue(id, out var supplier))
            {
                supplier.Tenders = [];
                return supplier;
            }
            supplier = new Supplier
            {
                Id = id,
                Name = id.ToString()
            };
            SupplierCache[id] = supplier;
            return supplier;
        }
    }

    private static string RandomTenderId => string.Join(
        string.Empty,
        Random.Shared.GetItems(
            Enumerable.Range(0, 10)
                .Select(c => c.ToString())
                .ToArray(), 6
            )
        );
}
