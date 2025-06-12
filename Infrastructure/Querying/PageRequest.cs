namespace Infrastructure.Querying;

public record PageRequest(
    int? Page,
    int? PageSize
);