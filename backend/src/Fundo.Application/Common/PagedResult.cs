namespace Fundo.Application.Common;

public sealed class PagedResult<T> : IPagedResult<T>
{
    public PagedResult()
    {
    }

    public PagedResult(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items.ToList();
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = CalculateTotalPages(totalCount, pageSize);
    }

    public List<T> Items { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public bool HasPreviousPage => PageNumber > 1 && TotalPages > 0;

    public bool HasNextPage => PageNumber < TotalPages;

    IReadOnlyList<T> IPagedResult<T>.Items => Items;

    private static int CalculateTotalPages(int totalCount, int pageSize)
    {
        return totalCount == 0 || pageSize <= 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
