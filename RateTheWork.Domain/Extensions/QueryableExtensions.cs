using System.Linq.Expressions;
using RateTheWork.Domain.Common;

namespace RateTheWork.Domain.Extensions;

/// <summary>
/// IQueryable için extension metodları
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Son X gün içinde oluşturulan kayıtları filtreler
    /// </summary>
    public static IQueryable<T> WhereRecent<T>(this IQueryable<T> query, int days = 30) where T : BaseEntity
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return query.Where(x => x.CreatedAt >= cutoffDate);
    }

    /// <summary>
    /// Tarih aralığına göre filtreler
    /// </summary>
    public static IQueryable<T> WhereDateRange<T>(this IQueryable<T> query, DateTime startDate, DateTime endDate)
        where T : BaseEntity
    {
        return query.Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);
    }

    /// <summary>
    /// Son N günün kayıtlarını getirir
    /// </summary>
    public static IQueryable<T> WhereLastDays<T>(this IQueryable<T> query, int days) where T : BaseEntity
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        return query.Where(x => x.CreatedAt >= startDate);
    }

    /// <summary>
    /// Oluşturma tarihine göre sıralar
    /// </summary>
    public static IQueryable<T> OrderByCreated<T>(this IQueryable<T> query, bool descending = true)
        where T : BaseEntity
    {
        return descending
            ? query.OrderByDescending(x => x.CreatedAt)
            : query.OrderBy(x => x.CreatedAt);
    }

    /// <summary>
    /// Sayfalama uygular
    /// </summary>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int page, int pageSize)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    /// <summary>
    /// Koşullu Where uygular
    /// </summary>
    public static IQueryable<T> WhereIf<T>
    (
        this IQueryable<T> query
        , bool condition
        , Expression<Func<T, bool>> predicate
    )
    {
        return condition ? query.Where(predicate) : query;
    }

    /// <summary>
    /// Dinamik sıralama uygular
    /// </summary>
    public static IQueryable<T> OrderByDynamic<T>
    (
        this IQueryable<T> query
        , string propertyName
        , bool descending = false
    )
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, propertyName);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = descending ? "OrderByDescending" : "OrderBy";
        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2);
        var genericMethod = method.MakeGenericMethod(typeof(T), property.Type);

        return (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, lambda })!;
    }

    /// <summary>
    /// Belirtilen property'leri içerir (eager loading)
    /// </summary>
    public static IQueryable<T> IncludeMultiple<T>
    (
        this IQueryable<T> query
        , params Expression<Func<T, object>>[] includes
    ) where T : class
    {
        // Bu metod Entity Framework Core için Include işlemini simüle eder
        // Gerçek implementasyon Infrastructure katmanında olacak
        return query;
    }

    /// <summary>
    /// Distinct by property
    /// </summary>
    public static IQueryable<T> DistinctBy<T, TKey>
    (
        this IQueryable<T> query
        , Expression<Func<T, TKey>> keySelector
    )
    {
        return query.GroupBy(keySelector).Select(g => g.First());
    }

    /// <summary>
    /// ID listesine göre filtreler
    /// </summary>
    public static IQueryable<T> WhereIdIn<T>(this IQueryable<T> query, IEnumerable<string> ids) where T : BaseEntity
    {
        return query.Where(x => ids.Contains(x.Id));
    }

    /// <summary>
    /// Full text search simülasyonu
    /// </summary>
    public static IQueryable<T> Search<T>
    (
        this IQueryable<T> query
        , string searchTerm
        , params Expression<Func<T, string>>[] searchProperties
    )
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combinedExpression = null;

        foreach (var property in searchProperties)
        {
            var propertyExpression = property.Body;
            // Property.Body'yi parameter ile değiştir
            var visitor = new ParameterReplacer(property.Parameters[0], parameter);
            propertyExpression = visitor.Visit(propertyExpression);

            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var searchExpression = Expression.Call(
                propertyExpression,
                containsMethod!,
                Expression.Constant(searchTerm));

            combinedExpression = combinedExpression == null
                ? searchExpression
                : Expression.OrElse(combinedExpression, searchExpression);
        }

        if (combinedExpression != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
            return query.Where(lambda);
        }

        return query;
    }

    // Helper class for parameter replacement
    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _newParameter;
        private readonly ParameterExpression _oldParameter;

        public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }
}
