using System.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Application.Common.Behaviors;

/// <summary>
/// Transaction pipeline behavior for handling database transactions
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior
    (
        ILogger<TransactionBehavior<TRequest, TResponse>> logger
        , IApplicationDbContext dbContext
    )
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle
    (
        TRequest request
        , RequestHandlerDelegate<TResponse> next
        , CancellationToken cancellationToken
    )
    {
        // Skip if request doesn't require transaction
        if (request is INoTransaction)
        {
            return await next();
        }

        var response = default(TResponse);
        var typeName = request.GetType().Name;

        try
        {
            if (_dbContext.HasActiveTransaction)
            {
                return await next();
            }

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                using (var transactionScope = new TransactionScope(
                           TransactionScopeOption.Required,
                           new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                           TransactionScopeAsyncFlowOption.Enabled))
                {
                    _logger.LogInformation("Begin transaction {TransactionId} for {CommandName}",
                        transaction.TransactionId, typeName);

                    try
                    {
                        response = await next();

                        await _dbContext.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);

                        transactionScope.Complete();

                        _logger.LogInformation("Commit transaction {TransactionId} for {CommandName}",
                            transaction.TransactionId, typeName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in transaction {TransactionId} for {CommandName}",
                            transaction.TransactionId, typeName);

                        await transaction.RollbackAsync(cancellationToken);
                        throw;
                    }
                }
            });

            return response!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling transaction for {CommandName}", typeName);
            throw;
        }
    }
}

/// <summary>
/// Marker interface for requests that should not use transactions
/// </summary>
public interface INoTransaction
{
}
