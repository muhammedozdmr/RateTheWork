using System.Linq.Expressions;
using System.Reflection;
using RateTheWork.Application.Common.Models;

namespace RateTheWork.Application.Common.Extensions;

/// <summary>
/// IQueryable extension methods for filtering, sorting and paging
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Apply filtering based on query parameters
    /// </summary>
    public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> query, QueryParameters parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.SearchTerm))
            return query;

        // Get searchable properties (string properties)
        var searchableProperties = typeof(T).GetProperties()
            .Where(p => p.PropertyType == typeof(string))
            .ToList();

        if (!searchableProperties.Any())
            return query;

        var searchTerm = parameters.SearchTerm.ToLower();
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combinedExpression = null;

        foreach (var property in searchableProperties)
        {
            var propertyAccess = Expression.Property(parameter, property);
            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            var toLowerCall = Expression.Call(propertyAccess, toLowerMethod!);
            var searchTermConstant = Expression.Constant(searchTerm);
            var containsCall = Expression.Call(toLowerCall, containsMethod!, searchTermConstant);

            combinedExpression = combinedExpression == null
                ? containsCall
                : Expression.OrElse(combinedExpression, containsCall);
        }

        if (combinedExpression != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
            query = query.Where(lambda);
        }

        return query;
    }

    /// <summary>
    /// Apply sorting based on query parameters
    /// </summary>
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, QueryParameters parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.SortBy))
            return query;

        // Check if property exists
        var property = typeof(T).GetProperty(parameters.SortBy,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
            return query;

        var sortExpression = parameters.SortDirection == SortDirection.Ascending
            ? $"{parameters.SortBy} ascending"
            : $"{parameters.SortBy} descending";

        return query.OrderBy(sortExpression);
    }

    /// <summary>
    /// Apply paging based on query parameters
    /// </summary>
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, QueryParameters parameters)
    {
        return query
            .Skip(parameters.Skip)
            .Take(parameters.PageSize);
    }

    /// <summary>
    /// Apply all query operations (filtering, sorting, paging)
    /// </summary>
    public static IQueryable<T> ApplyQuery<T>(this IQueryable<T> query, QueryParameters parameters)
    {
        return query
            .ApplyFiltering(parameters)
            .ApplySorting(parameters)
            .ApplyPaging(parameters);
    }

    /// <summary>
    /// Apply advanced filters
    /// </summary>
    public static IQueryable<T> ApplyAdvancedFilters<T>(this IQueryable<T> query, List<Filter> filters)
    {
        foreach (var filter in filters)
        {
            var property = typeof(T).GetProperty(filter.Field,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null) continue;

            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(parameter, property);

            Expression filterExpression = filter.Operator switch
            {
                FilterOperator.Equals => Expression.Equal(propertyAccess, Expression.Constant(filter.Value))
                , FilterOperator.NotEquals => Expression.NotEqual(propertyAccess, Expression.Constant(filter.Value))
                , FilterOperator.GreaterThan => Expression.GreaterThan(propertyAccess
                    , Expression.Constant(filter.Value))
                , FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(propertyAccess
                    , Expression.Constant(filter.Value))
                , FilterOperator.LessThan => Expression.LessThan(propertyAccess, Expression.Constant(filter.Value))
                , FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(propertyAccess
                    , Expression.Constant(filter.Value))
                , FilterOperator.Contains when property.PropertyType == typeof(string) =>
                    Expression.Call(propertyAccess, "Contains", Type.EmptyTypes, Expression.Constant(filter.Value))
                , FilterOperator.StartsWith when property.PropertyType == typeof(string) =>
                    Expression.Call(propertyAccess, "StartsWith", Type.EmptyTypes, Expression.Constant(filter.Value))
                , FilterOperator.EndsWith when property.PropertyType == typeof(string) =>
                    Expression.Call(propertyAccess, "EndsWith", Type.EmptyTypes, Expression.Constant(filter.Value))
                , _ => null!
            };

            if (filterExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(filterExpression, parameter);
                query = query.Where(lambda);
            }
        }

        return query;
    }

    /// <summary>
    /// Create paged result
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>
    (
        this IQueryable<T> query
        , QueryParameters parameters
        , CancellationToken cancellationToken = default
    )
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplyQuery(parameters)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(
            items,
            totalCount,
            parameters.PageNumber,
            parameters.PageSize);
    }
}

/// <summary>
/// Paged result model
/// </summary>
public class PagedResult<T>
{
    public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public List<T> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
