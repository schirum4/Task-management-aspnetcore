namespace TaskManagement.Api.Infrastructure;

public interface IHolidayProvider
{
    bool IsHoliday(DateTime date);
}
