using Microsoft.EntityFrameworkCore;

namespace RateTheWork.Application.Common.Mappings;

/// <summary>
/// Sayfalı veri listesi için generic wrapper.
/// API response'larında sayfalama bilgilerini taşır.
/// </summary>
/// <typeparam name="T">Liste elemanlarının tipi</typeparam>
public class PagedList<T>
{
    /// <summary>
    /// Sayfa elemanları
    /// </summary>
    public List<T> Items { get; }
    
    /// <summary>
    /// Toplam kayıt sayısı
    /// </summary>
    public int TotalCount { get; }
    
    /// <summary>
    /// Mevcut sayfa numarası (1'den başlar)
    /// </summary>
    public int PageNumber { get; }
    
    /// <summary>
    /// Sayfa başına kayıt sayısı
    /// </summary>
    public int PageSize { get; }
    
    /// <summary>
    /// Toplam sayfa sayısı
    /// </summary>
    public int TotalPages { get; }
    
    /// <summary>
    /// Önceki sayfa var mı?
    /// </summary>
    public bool HasPreviousPage { get; }
    
    /// <summary>
    /// Sonraki sayfa var mı?
    /// </summary>
    public bool HasNextPage { get; }

    /// <summary>
    /// PagedList constructor
    /// </summary>
    /// <param name="items">Sayfa elemanları</param>
    /// <param name="totalCount">Toplam kayıt sayısı</param>
    /// <param name="pageNumber">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    public PagedList(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        HasPreviousPage = pageNumber > 1;
        HasNextPage = pageNumber < TotalPages;
    }

    /// <summary>
    /// Boş bir sayfalı liste oluşturur
    /// </summary>
    public static PagedList<T> Empty(int pageNumber = 1, int pageSize = 10)
    {
        return new PagedList<T>(new List<T>(), 0, pageNumber, pageSize);
    }

    /// <summary>
    /// IQueryable'dan sayfalı liste oluşturur
    /// </summary>
    /// <param name="source">Veri kaynağı</param>
    /// <param name="pageNumber">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    public static async Task<PagedList<T>> CreateAsync(
        IQueryable<T> source, 
        int pageNumber, 
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }

    /// <summary>
    /// Bir PagedList'i başka bir tipe map'ler
    /// </summary>
    /// <typeparam name="TDestination">Hedef tip</typeparam>
    /// <param name="converter">Dönüştürme fonksiyonu</param>
    /// <returns>Yeni tipte PagedList</returns>
    public PagedList<TDestination> Map<TDestination>(Func<T, TDestination> converter)
    {
        var mappedItems = Items.Select(converter).ToList();
        return new PagedList<TDestination>(mappedItems, TotalCount, PageNumber, PageSize);
    }
}

/// <summary>
/// PagedList için extension metodları
/// </summary>
public static class PagedListExtensions
{
    /// <summary>
    /// PagedList'i API response formatına dönüştürür
    /// </summary>
    public static object ToApiResponse<T>(this PagedList<T> pagedList)
    {
        return new
        {
            items = pagedList.Items,
            pagination = new
            {
                totalCount = pagedList.TotalCount,
                pageNumber = pagedList.PageNumber,
                pageSize = pagedList.PageSize,
                totalPages = pagedList.TotalPages,
                hasPreviousPage = pagedList.HasPreviousPage,
                hasNextPage = pagedList.HasNextPage
            }
        };
    }

    /// <summary>
    /// Sayfalama parametrelerini validate eder
    /// </summary>
    public static (int pageNumber, int pageSize) ValidatePaginationParams(
        int pageNumber, 
        int pageSize, 
        int maxPageSize = 100)
    {
        // Sayfa numarası en az 1 olmalı
        if (pageNumber < 1)
            pageNumber = 1;

        // Sayfa boyutu 1-100 arasında olmalı
        if (pageSize < 1)
            pageSize = 10;
        else if (pageSize > maxPageSize)
            pageSize = maxPageSize;

        return (pageNumber, pageSize);
    }
}

/// <summary>
/// Sayfalama için request base class'ı
/// </summary>
public abstract record PaginatedRequest
{
    /// <summary>
    /// Sayfa numarası (varsayılan: 1)
    /// </summary>
    public int PageNumber { get; init; } = 1;
    
    /// <summary>
    /// Sayfa başına kayıt sayısı (varsayılan: 10, max: 100)
    /// </summary>
    public int PageSize { get; init; } = 10;
    
    /// <summary>
    /// Validate edilmiş sayfalama parametrelerini döner
    /// </summary>
    public (int pageNumber, int pageSize) GetValidatedPaginationParams()
    {
        return PagedListExtensions.ValidatePaginationParams(PageNumber, PageSize);
    }
}
