namespace Infrastructure.Querying;

public record QueryRequest(
    string? PageAfter,
    int? PageSize
);