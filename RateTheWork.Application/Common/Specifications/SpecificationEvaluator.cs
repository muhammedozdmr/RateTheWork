using Microsoft.EntityFrameworkCore;

namespace RateTheWork.Application.Common.Specifications;

/// <summary>
/// Specification evaluator for EF Core
/// </summary>
public static class SpecificationEvaluator<T> where T : class
{
    public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification)
    {
        var query = inputQuery;

        // Apply criteria
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply string includes
        query = specification.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply paging
        if (specification.IsPagingEnabled)
        {
            if (specification.Skip.HasValue)
                query = query.Skip(specification.Skip.Value);

            if (specification.Take.HasValue)
                query = query.Take(specification.Take.Value);
        }

        // Apply tracking
        if (specification.AsNoTracking)
        {
            query = query.AsNoTracking();
        }

        // Apply split query
        if (specification.IsSplitQuery)
        {
            query = query.AsSplitQuery();
        }

        return query;
    }
}

/// <summary>
/// Extension methods for specifications
/// </summary>
public static class SpecificationExtensions
{
    public static IQueryable<T> ApplySpecification<T>
    (
        this IQueryable<T> query
        , ISpecification<T> spec
    ) where T : class
    {
        return SpecificationEvaluator<T>.GetQuery(query, spec);
    }
}
