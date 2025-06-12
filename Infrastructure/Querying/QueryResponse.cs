namespace Infrastructure.Querying;

public record QueryResponse<T>(IEnumerable<T> Data, int Page, int PageSize, int Total);
