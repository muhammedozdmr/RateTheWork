using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Entities;

namespace RateTheWork.Application.Common.Behaviors;

/// <summary>
/// Auditing pipeline behavior for tracking user actions
/// </summary>
public class AuditingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<AuditingBehavior<TRequest, TResponse>> _logger;

    public AuditingBehavior
    (
        ILogger<AuditingBehavior<TRequest, TResponse>> logger
        , ICurrentUserService currentUserService
        , IApplicationDbContext context
        , IDateTimeService dateTimeService
    )
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _context = context;
        _dateTimeService = dateTimeService;
    }

    public async Task<TResponse> Handle
    (
        TRequest request
        , RequestHandlerDelegate<TResponse> next
        , CancellationToken cancellationToken
    )
    {
        // Skip if not auditable
        if (request is not IAuditableRequest)
        {
            return await next();
        }

        var requestName = request.GetType().Name;
        var userId = _currentUserService.UserId;
        var userName = _currentUserService.UserName;
        var timestamp = _dateTimeService.Now;

        // Serialize request for audit
        string requestData;
        try
        {
            requestData = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                WriteIndented = false, MaxDepth = 2 // Prevent circular references
            });
        }
        catch
        {
            requestData = $"{{ \"Type\": \"{requestName}\" }}";
        }

        // Log start
        _logger.LogInformation(
            "User {UserName} ({UserId}) started {RequestName} at {Timestamp}",
            userName, userId, requestName, timestamp);

        var startTime = _dateTimeService.Now;
        var success = false;
        string? errorMessage = null;
        Exception? thrownException = null;

        try
        {
            var response = await next();
            success = true;
            
            var duration = _dateTimeService.Now - startTime;
            await LogAuditIfNeeded(request, requestData, requestName, userId, userName, 
                duration, success, errorMessage, cancellationToken);
                
            return response;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            thrownException = ex;
            
            var duration = _dateTimeService.Now - startTime;
            await LogAuditIfNeeded(request, requestData, requestName, userId, userName, 
                duration, success, errorMessage, cancellationToken);
                
            throw;
        }
    }
    
    private async Task LogAuditIfNeeded(TRequest request, string requestData, string requestName, 
        string? userId, string? userName, TimeSpan duration, bool success, string? errorMessage, 
        CancellationToken cancellationToken)
    {
        // Skip audit logging for non-admin users
        if (!_currentUserService.Roles.Contains("Admin"))
        {
            _logger.LogInformation(
                "User {UserName} ({UserId}) completed {RequestName} in {Duration}ms with result: {Success}",
                userName, userId, requestName, duration.TotalMilliseconds, success);
            return;
        }

        // Create audit log entry for admin actions
        var entityType = GetEntityType(request);
        var entityId = GetEntityId(request);
        
        if (!string.IsNullOrEmpty(entityType) && !string.IsNullOrEmpty(entityId) && !string.IsNullOrEmpty(userId))
        {
            var auditLog = AuditLog.Create(
                adminUserId: userId,
                actionType: requestName,
                entityType: entityType,
                entityId: entityId,
                details: requestData,
                ipAddress: GetIpAddress(),
                userAgent: GetUserAgent()
            );

            if (!success)
            {
                auditLog.MarkAsFailed(errorMessage ?? "Unknown error");
            }

            _context.AuditLogs.Add(auditLog);
            
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save audit log for {RequestName}", requestName);
            }
        }

        // Log completion
        _logger.LogInformation(
            "User {UserName} ({UserId}) completed {RequestName} in {Duration}ms with result: {Success}",
            userName, userId, requestName, duration.TotalMilliseconds, success);
    }
    
    private string? GetIpAddress()
    {
        // This would typically come from HttpContext in the infrastructure layer
        // For now, return null as it should be injected via ICurrentUserService
        return null;
    }
    
    private string? GetUserAgent()
    {
        // This would typically come from HttpContext in the infrastructure layer
        // For now, return null as it should be injected via ICurrentUserService
        return null;
    }

    private static string? GetEntityType(TRequest request)
    {
        if (request is IAuditableRequest auditableRequest)
        {
            return auditableRequest.EntityType;
        }

        // Try to extract from request type name
        var typeName = request.GetType().Name;
        var entityName = typeName
            .Replace("Command", "")
            .Replace("Query", "")
            .Replace("Create", "")
            .Replace("Update", "")
            .Replace("Delete", "")
            .Replace("Get", "");

        return string.IsNullOrEmpty(entityName) ? null : entityName;
    }

    private static string? GetEntityId(TRequest request)
    {
        if (request is IAuditableRequest auditableRequest)
        {
            return auditableRequest.EntityId;
        }

        // Try to extract Id property via reflection
        var idProperty = request.GetType().GetProperty("Id")
                         ?? request.GetType().GetProperty($"{GetEntityType(request)}Id");

        return idProperty?.GetValue(request)?.ToString();
    }

    private static string? SerializeResponse(TResponse response)
    {
        try
        {
            return JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = false, MaxDepth = 2
            });
        }
        catch
        {
            return response?.GetType().Name;
        }
    }

    private static Dictionary<string, object?>? GetChanges(TRequest request, TResponse? response)
    {
        if (request is not IAuditableRequest auditableRequest)
            return null;

        return auditableRequest.GetAuditChanges(response);
    }
}

/// <summary>
/// Interface for auditable requests
/// </summary>
public interface IAuditableRequest
{
    /// <summary>
    /// Entity type being audited
    /// </summary>
    string? EntityType { get; }

    /// <summary>
    /// Entity ID being audited
    /// </summary>
    string? EntityId { get; }

    /// <summary>
    /// Get audit changes
    /// </summary>
    Dictionary<string, object?>? GetAuditChanges(object? response);
}

/// <summary>
/// Base auditable request
/// </summary>
public abstract class AuditableRequest<TResponse> : IRequest<TResponse>, IAuditableRequest
{
    public virtual string? EntityType { get; protected set; }
    public virtual string? EntityId { get; protected set; }

    public virtual Dictionary<string, object?>? GetAuditChanges(object? response)
    {
        return null;
    }
}
