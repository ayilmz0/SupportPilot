
namespace SupportPilot.Application.Common.Pagination;

public sealed class PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; }
    public int CurrentPage { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public PagedResponse(
        IReadOnlyList<T> items,
        int currentPage,
        int pageSize,
        int totalCount)
    {
        Items = items;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(
            (double)totalCount / pageSize);
    }
}