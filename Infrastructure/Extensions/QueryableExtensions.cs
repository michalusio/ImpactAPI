using System.Linq.Expressions;

namespace Infrastructure.Extensions;
public static class QueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
    {
        if (condition)
        {
            return query.Where(predicate);
        }
        return query;
    }

    public static IQueryable<T> OrderedBy<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> orderField, bool descending)
    {
        if (descending)
        {
            return query.OrderByDescending(orderField);
        }
        else
        {
            return query.OrderBy(orderField);
        }
    }
}
