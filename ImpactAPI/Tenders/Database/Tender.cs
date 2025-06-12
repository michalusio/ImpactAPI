using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ImpactAPI.Tenders.Database;

[Index(nameof(Date))]
[Index(nameof(AwardedValueInEuro))]
public class Tender
{
    [Key, MaxLength(10)] public required string Id { get; set; }
    public required DateTime Date { get; set; }
    public required string Title { get; set; }
    public required string? Description { get; set; }
    [Precision(18, 2)] public required decimal AwardedValueInEuro { get; set; }
    public ICollection<Supplier> Suppliers { get; set; } = [];
}
