using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Models;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; }

    public int TotalCount { get; init; }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalPages { get; init; }

    public PagedResult()
    {
        Items = Array.Empty<T>();
        PageNumber = 1;
        PageSize = 10;
        TotalCount = 0;
        TotalPages = 0;
    }

    public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        var safePageSize = pageSize <= 0 ? 1 : pageSize;

        Items = items.ToList();
        TotalCount = totalCount;
        PageNumber = pageNumber <= 0 ? 1 : pageNumber;
        PageSize = safePageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)safePageSize);
    }
}
