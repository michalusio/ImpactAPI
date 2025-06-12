using ImpactAPI.Tenders.Database;
using ImpactAPI.Tenders.Service;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ImpactAPI.Tests;

public class TenderServiceTests
{
    private TestContainerDatabase Database;
    private TendersDbContext DatabaseContext;

    [SetUp]
    public async Task Setup()
    {
        Database = new TestContainerDatabase("Tenders", withVolumeMount: false);
        await Database.PreStartDatabase();

        var dbOptions = new DbContextOptionsBuilder()
            .UseSqlServer(Database.ConnectionString)
            .Options;
        DatabaseContext = new TendersDbContext(dbOptions);
        await DatabaseContext.Database.MigrateAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await DatabaseContext.DisposeAsync();
        await Database.StopAsync(CancellationToken.None);
        await Database.DisposeAsync();
    }

    [Test]
    public async Task Should_RetrieveTenderById()
    {
        // Arrange
        var dbTender = TenderGenerator.RandomTender;
        DatabaseContext.Add(dbTender);
        await DatabaseContext.SaveChangesAsync();

        // Act
        var service = new TenderService(DatabaseContext);
        var tender = await service.GetTenderById(dbTender.Id, CancellationToken.None);

        // Assert
        Assert.That(tender.Title, Is.EqualTo(dbTender.Title));
    }

    [Test]
    public async Task Should_ThrowOnUnknownTenderId()
    {
        // Arrange
        var dbTender = TenderGenerator.RandomTender;
        DatabaseContext.Add(dbTender);
        await DatabaseContext.SaveChangesAsync();

        // Act
        var service = new TenderService(DatabaseContext);
        try
        {
            await service.GetTenderById("123", CancellationToken.None);
            Assert.Fail();
        }
        // Assert
        catch (AssertionException)
        {
            // Passthrough
            throw;
        }
        catch (Exception)
        {
            Assert.Pass();
        }
    }

    [Test]
    public async Task Should_RetrieveOrderedById_WhenSortFieldIsNotSpecified()
    {
        // Arrange
        var dbTenders = Enumerable.Range(0, 10)
            .Select(_ => TenderGenerator.RandomNonDuplicateTender)
            .ToList();
        DatabaseContext.Tenders.AddRange(dbTenders);

        await DatabaseContext.SaveChangesAsync();

        // Act
        var service = new TenderService(DatabaseContext);
        var tenders = await service.GetTenders(new(
            Page: null,
            PageSize: null,
            DateFrom: null,
            DateTo: null,
            SupplierId: null,
            AwardedValueInEuroFrom: null,
            AwardedValueInEuroTo: null,
            SortField: null,
            SortDescending: null
        ), CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(tenders.Total, Is.EqualTo(10));
            Assert.That(tenders.PageSize, Is.EqualTo(100));
            Assert.That(tenders.Page, Is.EqualTo(1));
            Assert.That(tenders.Data, Is.Ordered.Ascending.By(nameof(TenderReadModel.Id)));
        });
    }

    [Test]
    public async Task Should_RetrieveOrderedByIdDescending_WhenSortFieldIsNotSpecified_ButSortDescendingIs()
    {
        // Arrange
        var dbTenders = Enumerable.Range(0, 10)
            .Select(_ => TenderGenerator.RandomNonDuplicateTender)
            .ToList();
        DatabaseContext.Tenders.AddRange(dbTenders);

        await DatabaseContext.SaveChangesAsync();

        // Act
        var service = new TenderService(DatabaseContext);
        var tenders = await service.GetTenders(new(
            Page: null,
            PageSize: null,
            DateFrom: null,
            DateTo: null,
            SupplierId: null,
            AwardedValueInEuroFrom: null,
            AwardedValueInEuroTo: null,
            SortField: null,
            SortDescending: true
        ), CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(tenders.Total, Is.EqualTo(10));
            Assert.That(tenders.PageSize, Is.EqualTo(100));
            Assert.That(tenders.Page, Is.EqualTo(1));
            Assert.That(tenders.Data, Is.Ordered.Descending.By(nameof(TenderReadModel.Id)));
        });
    }

    [Test]
    public async Task Should_RetrieveOrderedByDate_WhenSortFieldIsSpecifiedAsDate()
    {
        // Arrange
        var dbTenders = Enumerable.Range(0, 10)
            .Select(_ => TenderGenerator.RandomNonDuplicateTender)
            .ToList();
        DatabaseContext.Tenders.AddRange(dbTenders);

        await DatabaseContext.SaveChangesAsync();

        // Act
        var service = new TenderService(DatabaseContext);
        var tenders = await service.GetTenders(new(
            Page: null,
            PageSize: null,
            DateFrom: null,
            DateTo: null,
            SupplierId: null,
            AwardedValueInEuroFrom: null,
            AwardedValueInEuroTo: null,
            SortField: TenderSortField.Date,
            SortDescending: null
        ), CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(tenders.Total, Is.EqualTo(10));
            Assert.That(tenders.PageSize, Is.EqualTo(100));
            Assert.That(tenders.Page, Is.EqualTo(1));
            Assert.That(tenders.Data, Is.Ordered.Ascending.By(nameof(TenderReadModel.Date)));
        });
    }

    [Test]
    public async Task Should_RetrieveTwoPages_OrderedByDate()
    {
        // Arrange
        var dbTenders = Enumerable.Range(0, 10)
            .Select(_ => TenderGenerator.RandomNonDuplicateTender)
            .ToList();
        DatabaseContext.Tenders.AddRange(dbTenders);

        await DatabaseContext.SaveChangesAsync();

        // Act
        var service = new TenderService(DatabaseContext);
        var tenders = await service.GetTenders(new(
            Page: null,
            PageSize: 5,
            DateFrom: null,
            DateTo: null,
            SupplierId: null,
            AwardedValueInEuroFrom: null,
            AwardedValueInEuroTo: null,
            SortField: TenderSortField.Date,
            SortDescending: null
        ), CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(tenders.Total, Is.EqualTo(10));
            Assert.That(tenders.PageSize, Is.EqualTo(5));
            Assert.That(tenders.Page, Is.EqualTo(1));
            Assert.That(tenders.Data, Is.Ordered.Ascending.By(nameof(TenderReadModel.Date)));
        });

        // Act 2
        var tendersSecond = await service.GetTenders(new(
            Page: 2,
            PageSize: 5,
            DateFrom: null,
            DateTo: null,
            SupplierId: null,
            AwardedValueInEuroFrom: null,
            AwardedValueInEuroTo: null,
            SortField: TenderSortField.Date,
            SortDescending: null
        ), CancellationToken.None);

        // Assert 2
        Assert.Multiple(() =>
        {
            Assert.That(tendersSecond.Total, Is.EqualTo(10));
            Assert.That(tendersSecond.PageSize, Is.EqualTo(5));
            Assert.That(tendersSecond.Page, Is.EqualTo(2));
            Assert.That(tendersSecond.Data, Is.Ordered.Ascending.By(nameof(TenderReadModel.Date)));
            Assert.That(tendersSecond.Data, Has.All.Property(nameof(TenderReadModel.Date)).GreaterThanOrEqualTo(tenders.Data.Last().Date));
            foreach (var t1 in tenders.Data)
            {
                foreach (var t2 in tendersSecond.Data)
                {
                    if (t1.Id == t2.Id)
                    {
                        Assert.Fail("Two pages should not have the same item");
                    }
                }
            }
        });
    }

    [Test]
    public async Task Should_RetrieveFilteredByEuro()
    {
        // Arrange
        var dbTenders = Enumerable.Range(0, 10)
            .Select(_ => TenderGenerator.RandomNonDuplicateTender)
            .ToList();
        var averageEuroValue = dbTenders.Average(t => t.AwardedValueInEuro);
        DatabaseContext.Tenders.AddRange(dbTenders);

        await DatabaseContext.SaveChangesAsync();

        // Act
        var service = new TenderService(DatabaseContext);
        var tenders = await service.GetTenders(new(
            Page: null,
            PageSize: null,
            DateFrom: null,
            DateTo: null,
            SupplierId: null,
            AwardedValueInEuroFrom: averageEuroValue,
            AwardedValueInEuroTo: null,
            SortField: null,
            SortDescending: null
        ), CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(tenders.Total, Is.EqualTo(dbTenders.Count(t => t.AwardedValueInEuro >= averageEuroValue)));
            Assert.That(tenders.PageSize, Is.EqualTo(100));
            Assert.That(tenders.Page, Is.EqualTo(1));
            Assert.That(tenders.Data, Has.All.Property(nameof(TenderReadModel.AwardedValueInEuro)).GreaterThanOrEqualTo(averageEuroValue));
        });
    }
}