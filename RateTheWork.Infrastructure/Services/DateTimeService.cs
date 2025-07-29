using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    private static readonly TimeZoneInfo TurkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyTimeZone);

    public DateTime Today => Now.Date;
}
