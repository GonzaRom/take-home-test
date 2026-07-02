namespace Fundo.Application.Common;

public interface IPagedResult<out T>
{
    IReadOnlyList<T> Items { get; }

    int PageNumber { get; }

    int PageSize { get; }

    int TotalCount { get; }

    int TotalPages { get; }

    bool HasPreviousPage { get; }

    bool HasNextPage { get; }
}
