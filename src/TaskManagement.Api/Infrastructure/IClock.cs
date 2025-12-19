namespace TaskManagement.Api.Infrastructure;

public interface IClock
{
    DateTime Today { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime Today => DateTime.Today;
}
