namespace Relay.SharedKernel.Application;

/// <summary>
/// Pagination envelope used by list queries. PageNumber is 1-based.
/// </summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }

    public int TotalPages => PageSize == 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        Items = items ?? throw new ArgumentNullException(nameof(items));
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize < 1 ? 1 : pageSize;
        TotalCount = totalCount < 0 ? 0 : totalCount;
    }
}

/// <summary>
/// Factory/helper methods for PagedResult.
/// Avoids static members on generic types.
/// </summary>
public static class PagedResult
{
    public static PagedResult<T> Create<T>(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        return new PagedResult<T>(items, pageNumber, pageSize, totalCount);
    }

    public static PagedResult<T> Empty<T>(
        int pageNumber = 1,
        int pageSize = 25)
    {
        return new PagedResult<T>(
            Array.Empty<T>(),
            pageNumber,
            pageSize,
            0);
    }
}
