using System.Linq.Expressions;

namespace RateTheWork.Application.Common.Specifications;

/// <summary>
/// Specification pattern interface
/// </summary>
public interface ISpecification<T>
{
    /// <summary>
    /// Criteria expression
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Include expressions
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Include string paths
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Order by expression
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Order by descending expression
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Take count
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Skip count
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Paging enabled
    /// </summary>
    bool IsPagingEnabled { get; }

    /// <summary>
    /// Split query
    /// </summary>
    bool IsSplitQuery { get; }

    /// <summary>
    /// As no tracking
    /// </summary>
    bool AsNoTracking { get; }
}

/// <summary>
/// Base specification implementation
/// </summary>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    protected BaseSpecification()
    {
    }

    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int? Take { get; private set; }
    public int? Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }
    public bool IsSplitQuery { get; private set; }
    public bool AsNoTracking { get; private set; } = true;

    protected void AddCriteria(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }

    protected void ApplySplitQuery()
    {
        IsSplitQuery = true;
    }

    protected void ApplyTracking()
    {
        AsNoTracking = false;
    }
}
