namespace RateTheWork.Domain.ValueObjects.Common;

/// <summary>
/// Tarih aralığı
/// </summary>
public sealed class DateRange : ValueObject
{
    private DateRange(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Başlangıç tarihi
    /// </summary>
    public DateTime StartDate { get; }

    /// <summary>
    /// Bitiş tarihi
    /// </summary>
    public DateTime EndDate { get; }

    /// <summary>
    /// Aralık süresi (gün cinsinden)
    /// </summary>
    public int DurationInDays => (EndDate - StartDate).Days + 1;

    /// <summary>
    /// Tarih aralığı oluşturur
    /// </summary>
    public static DateRange Create(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("Başlangıç tarihi bitiş tarihinden sonra olamaz.");

        return new DateRange(startDate, endDate);
    }

    /// <summary>
    /// Bugünden itibaren belirli gün sayısı için aralık oluşturur
    /// </summary>
    public static DateRange CreateFromToday(int days)
    {
        if (days < 1)
            throw new ArgumentOutOfRangeException(nameof(days), "Gün sayısı 1'den küçük olamaz.");

        var today = DateTime.Today;
        return new DateRange(today, today.AddDays(days - 1));
    }

    /// <summary>
    /// Bu ay için tarih aralığı oluşturur
    /// </summary>
    public static DateRange CreateForCurrentMonth()
    {
        var today = DateTime.Today;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
        return new DateRange(firstDayOfMonth, lastDayOfMonth);
    }

    /// <summary>
    /// Bu yıl için tarih aralığı oluşturur
    /// </summary>
    public static DateRange CreateForCurrentYear()
    {
        var today = DateTime.Today;
        var firstDayOfYear = new DateTime(today.Year, 1, 1);
        var lastDayOfYear = new DateTime(today.Year, 12, 31);
        return new DateRange(firstDayOfYear, lastDayOfYear);
    }

    /// <summary>
    /// Belirli bir tarihin aralık içinde olup olmadığını kontrol eder
    /// </summary>
    public bool Contains(DateTime date)
    {
        return date >= StartDate && date <= EndDate;
    }

    /// <summary>
    /// İki tarih aralığının kesişip kesişmediğini kontrol eder
    /// </summary>
    public bool Overlaps(DateRange other)
    {
        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }

    /// <summary>
    /// İki tarih aralığının kesişim aralığını döndürür
    /// </summary>
    public DateRange? GetIntersection(DateRange other)
    {
        if (!Overlaps(other))
            return null;

        var intersectionStart = StartDate > other.StartDate ? StartDate : other.StartDate;
        var intersectionEnd = EndDate < other.EndDate ? EndDate : other.EndDate;

        return new DateRange(intersectionStart, intersectionEnd);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }

    public override string ToString()
    {
        return $"{StartDate:yyyy-MM-dd} - {EndDate:yyyy-MM-dd}";
    }
}
