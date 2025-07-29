namespace RateTheWork.Application.Common.Models;

/// <summary>
/// Sayfalama için generic liste modeli
/// </summary>
public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public int PageSize { get; }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        PageSize = pageSize;
        Items = items;
    }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    
    /// <summary>
    /// IQueryable'dan sayfalanmış liste oluşturur
    /// </summary>
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await Task.Run(() => source.Count());
        var items = await Task.Run(() => source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList());
        
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
    
    /// <summary>
    /// Liste'den sayfalanmış liste oluşturur
    /// </summary>
    public static PaginatedList<T> Create(List<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count;
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}