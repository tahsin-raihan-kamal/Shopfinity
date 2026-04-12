namespace Shopfinity.Application.Common;

/// <summary>
/// Wraps a paginated list with metadata so the frontend knows total pages.
/// Used by ProductService.SearchProductsAsync.
/// </summary>
public class PaginatedResult<T>
{
    public IEnumerable<T> Items      { get; set; } = [];
    public int            TotalCount { get; set; }
    public int            PageNumber { get; set; }
    public int            PageSize   { get; set; }
    public int            TotalPages => PageSize > 0
        ? (int)Math.Ceiling((double)TotalCount / PageSize)
        : 0;
    public bool           HasNext => PageNumber < TotalPages;
    public bool           HasPrev => PageNumber > 1;
}
