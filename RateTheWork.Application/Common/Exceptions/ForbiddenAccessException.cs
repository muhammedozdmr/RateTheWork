namespace RateTheWork.Application.Common.Exceptions;

/// <summary>
/// Yetkilendirme hatalarını temsil eden exception
/// </summary>
public class ForbiddenAccessException : Exception
{
    /// <summary>
    /// ForbiddenAccessException oluşturur
    /// </summary>
    public ForbiddenAccessException() : base("Bu işlem için yetkiniz yok.")
    {
    }
    
    /// <summary>
    /// Özel mesaj ile ForbiddenAccessException oluşturur
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    public ForbiddenAccessException(string message) : base(message)
    {
    }
}
