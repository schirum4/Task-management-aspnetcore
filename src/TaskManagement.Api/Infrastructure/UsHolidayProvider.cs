namespace TaskManagement.Api.Infrastructure;

/// <summary>
/// US federal holiday provider.
/// Includes US federal holidays with "observed" rules when the date falls on a weekend.
/// This is intentionally lightweight for a take-home assignment.
/// </summary>
public sealed class UsHolidayProvider : IHolidayProvider
{
    public bool IsHoliday(DateTime date)
    {
        var d = date.Date;
        var year = d.Year;

        var holidays = new HashSet<DateTime>
        {
            Observed(new DateTime(year, 1, 1)),
            NthWeekdayOfMonth(year, 1, DayOfWeek.Monday, 3),
            NthWeekdayOfMonth(year, 2, DayOfWeek.Monday, 3),
            LastWeekdayOfMonth(year, 5, DayOfWeek.Monday),
            Observed(new DateTime(year, 6, 19)),
            Observed(new DateTime(year, 7, 4)),
            NthWeekdayOfMonth(year, 9, DayOfWeek.Monday, 1),
            NthWeekdayOfMonth(year, 10, DayOfWeek.Monday, 2),
            Observed(new DateTime(year, 11, 11)),
            NthWeekdayOfMonth(year, 11, DayOfWeek.Thursday, 4),
            Observed(new DateTime(year, 12, 25))
        };

        return holidays.Contains(d);
    }

    private static DateTime Observed(DateTime holiday)
    {
        return holiday.DayOfWeek switch
        {
            DayOfWeek.Saturday => holiday.AddDays(-1).Date,
            DayOfWeek.Sunday => holiday.AddDays(1).Date,
            _ => holiday.Date
        };
    }

    private static DateTime NthWeekdayOfMonth(int year, int month, DayOfWeek weekday, int nth)
    {
        var first = new DateTime(year, month, 1);
        var offset = ((int)weekday - (int)first.DayOfWeek + 7) % 7;
        return first.AddDays(offset + (nth - 1) * 7).Date;
    }

    private static DateTime LastWeekdayOfMonth(int year, int month, DayOfWeek weekday)
    {
        var firstNextMonth = new DateTime(year, month, 1).AddMonths(1);
        var lastDay = firstNextMonth.AddDays(-1);
        var offset = ((int)lastDay.DayOfWeek - (int)weekday + 7) % 7;
        return lastDay.AddDays(-offset).Date;
    }
}
