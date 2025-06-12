using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImpactAPI.Tenders.Database;

public class Supplier
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)] public required int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<Tender> Tenders { get; set; } = [];
}
