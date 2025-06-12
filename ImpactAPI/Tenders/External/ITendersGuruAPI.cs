using System.Text.Json.Serialization;
using Refit;

namespace ImpactAPI.Tenders.External;

public interface ITendersGuruAPI
{
    [Get("/tenders")]
    public Task<TendersListDto> GetTenders([Query] int page);

    public record TendersListDto(
        [property: JsonPropertyName("data")] IEnumerable<TenderDto> Tenders
    );

    public record TenderDto(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("date")] DateTime Date,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("awarded_value_eur")] string AwardedValueInEuro,
        [property: JsonPropertyName("awarded")] IEnumerable<TenderAwardDto> Awards
    );

    public record TenderAwardDto(
        [property: JsonPropertyName("suppliers")] IEnumerable<TenderSupplierDto> Suppliers
    );

    public record TenderSupplierDto(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("id")] int Id
    );
}
