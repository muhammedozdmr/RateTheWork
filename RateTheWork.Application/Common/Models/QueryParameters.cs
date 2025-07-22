namespace RateTheWork.Application.Common.Models;

/// <summary>
/// Base query parameters for filtering, sorting and paging
/// </summary>
public class QueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;
    private string? _searchTerm;

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    /// <summary>
    /// Search term
    /// </summary>
    public string? SearchTerm
    {
        get => _searchTerm;
        set => _searchTerm = value?.Trim();
    }

    /// <summary>
    /// Sort by field
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction
    /// </summary>
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

    /// <summary>
    /// Include soft deleted items
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;

    /// <summary>
    /// Skip count for paging
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;
}

/// <summary>
/// Sort direction
/// </summary>
public enum SortDirection
{
    Ascending = 0
    , Descending = 1
}

/// <summary>
/// Filter operator
/// </summary>
public enum FilterOperator
{
    Equals
    , NotEquals
    , GreaterThan
    , GreaterThanOrEqual
    , LessThan
    , LessThanOrEqual
    , Contains
    , StartsWith
    , EndsWith
    , In
    , NotIn
    , Between
}

/// <summary>
/// Dynamic filter
/// </summary>
public class Filter
{
    public string Field { get; set; } = string.Empty;
    public FilterOperator Operator { get; set; }
    public object? Value { get; set; }
    public object? Value2 { get; set; } // For between operator
}

/// <summary>
/// Advanced query parameters with dynamic filters
/// </summary>
public class AdvancedQueryParameters : QueryParameters
{
    /// <summary>
    /// Dynamic filters
    /// </summary>
    public List<Filter> Filters { get; set; } = new();

    /// <summary>
    /// Fields to include
    /// </summary>
    public List<string> IncludeFields { get; set; } = new();

    /// <summary>
    /// Fields to exclude
    /// </summary>
    public List<string> ExcludeFields { get; set; } = new();

    /// <summary>
    /// Group by fields
    /// </summary>
    public List<string> GroupBy { get; set; } = new();

    /// <summary>
    /// Enable distinct
    /// </summary>
    public bool Distinct { get; set; }
}
