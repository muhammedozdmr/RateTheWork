namespace RateTheWork.Application.Common.Exceptions;

/// <summary>
/// Yetkisiz erişim exception'ı
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException() : base("Bu işlem için yetkiniz yok.")
    {
    }

    public UnauthorizedException(string message) : base(message)
    {
    }

    public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}