using TaskManagement.Api.Infrastructure;

namespace TaskManagement.Tests.Fixtures;

public sealed class FakeHolidayProvider : IHolidayProvider
{
    private readonly HashSet<DateTime> _holidays;

    public FakeHolidayProvider(IEnumerable<DateTime>? holidays = null)
    {
        _holidays = (holidays ?? Enumerable.Empty<DateTime>()).Select(d => d.Date).ToHashSet();
    }

    public bool IsHoliday(DateTime date) => _holidays.Contains(date.Date);
}
