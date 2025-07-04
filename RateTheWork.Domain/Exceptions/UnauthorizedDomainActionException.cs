namespace RateTheWork.Domain.Exceptions;

/// <summary>
/// Yetkisiz işlem exception'ı
/// </summary>
public class UnauthorizedDomainActionException : DomainException
{
    public string Action { get; }
    public string Resource { get; }

    public UnauthorizedDomainActionException(string action, string resource)
        : base($"Unauthorized to perform '{action}' on resource '{resource}'.")
    {
        Action = action;
        Resource = resource;
    }

    public UnauthorizedDomainActionException(string message)
        : base(message)
    {
        Action = "Unknown";
        Resource = "Unknown";
    }
}
