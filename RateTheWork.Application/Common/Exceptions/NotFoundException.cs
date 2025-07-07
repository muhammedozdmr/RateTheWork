namespace RateTheWork.Application.Common.Exceptions;

/// <summary>
/// Aranan kayıt bulunamadığında fırlatılan exception
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// NotFoundException oluşturur
    /// </summary>
    public NotFoundException()
        : base()
    {
    }

    /// <summary>
    /// Mesaj ile NotFoundException oluşturur
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Mesaj ve inner exception ile NotFoundException oluşturur
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    /// <param name="innerException">İç exception</param>
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Entity adı ve key ile NotFoundException oluşturur
    /// </summary>
    /// <param name="name">Entity adı</param>
    /// <param name="key">Aranan key değeri</param>
    public NotFoundException(string name, object? key)
        : base($"Entity \"{name}\" ({key}) bulunamadı.")
    {
    }
}
