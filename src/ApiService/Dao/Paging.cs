namespace ApiService.Dao;

public static class Paging
{
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int page, int pageSize) =>
        query.Skip(Math.Max(0, page) * pageSize).Take(pageSize);

    public static string Term(string value) => value.Trim().ToLowerInvariant();
}
