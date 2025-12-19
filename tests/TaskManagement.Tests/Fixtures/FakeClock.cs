using TaskManagement.Api.Infrastructure;

namespace TaskManagement.Tests.Fixtures;

public sealed class FakeClock : IClock
{
    public DateTime Today { get; set; }
}
