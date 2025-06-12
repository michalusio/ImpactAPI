namespace Infrastructure.Querying;

public record QueryResponse<T>(IEnumerable<T> Data, string NextPage, int PageSize, int Total);
