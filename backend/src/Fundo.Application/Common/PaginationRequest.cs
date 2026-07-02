namespace Fundo.Application.Common;

public sealed class PaginationRequest
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    public int PageNumber { get; set; } = DefaultPageNumber;

    public int PageSize { get; set; } = DefaultPageSize;

    public string? GetValidationError()
    {
        if (PageNumber < 1)
        {
            return "pageNumber must be greater than or equal to 1.";
        }

        if (PageSize < 1 || PageSize > MaxPageSize)
        {
            return $"pageSize must be between 1 and {MaxPageSize}.";
        }

        return null;
    }
}
