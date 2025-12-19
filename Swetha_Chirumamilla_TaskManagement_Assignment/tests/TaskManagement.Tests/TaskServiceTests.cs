using TaskManagement.Api.Domain;
using TaskManagement.Api.Repositories;
using TaskManagement.Api.Services;
using TaskManagement.Tests.Fixtures;
using Xunit;

namespace TaskManagement.Tests;

public sealed class TaskServiceTests
{
    private readonly InMemoryTaskRepository _repo = new();
    private readonly FakeClock _clock = new() { Today = new DateTime(2025, 12, 16) };

    [Fact]
    public async Task Create_ShouldFail_WhenDueDateIsInPast()
    {
        var service = new TaskService(_repo, new FakeHolidayProvider(), _clock);

        var task = new TaskItem
        {
            Name = "T1",
            Description = "D",
            StartDate = _clock.Today,
            DueDate = _clock.Today.AddDays(-1),
            Priority = Priority.Medium,
            Status = Status.New
        };

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(task));
        Assert.Contains("in the past", ex.Message);
    }

    [Fact]
    public async Task Create_ShouldFail_WhenDueDateIsWeekend()
    {
        var service = new TaskService(_repo, new FakeHolidayProvider(), _clock);

        // 2025-12-20 is Saturday
        var task = new TaskItem
        {
            Name = "T2",
            DueDate = new DateTime(2025, 12, 20),
            StartDate = _clock.Today,
            Priority = Priority.Low,
            Status = Status.New
        };

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(task));
        Assert.Contains("weekend", ex.Message);
    }

    [Fact]
    public async Task Create_ShouldFail_WhenDueDateIsHoliday()
    {
        var holiday = new DateTime(2025, 12, 25);
        var service = new TaskService(_repo, new FakeHolidayProvider(new[] { holiday }), _clock);

        var task = new TaskItem
        {
            Name = "T3",
            DueDate = holiday,
            StartDate = _clock.Today,
            Priority = Priority.Low,
            Status = Status.New
        };

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(task));
        Assert.Contains("holiday", ex.Message);
    }

    
    [Fact]
    public async Task Create_ShouldFail_WhenDueDateIsObservedUsFederalHoliday()
    {
        // Example: July 4, 2026 is Saturday, so the observed holiday is Friday July 3, 2026.
        var clock = new FakeClock { Today = new DateTime(2026, 7, 1) };
        var service = new TaskService(new InMemoryTaskRepository(), new TaskManagement.Api.Infrastructure.UsHolidayProvider(), clock);

        var task = new TaskItem
        {
            Name = "Observed Holiday Test",
            Description = "D",
            StartDate = clock.Today,
            DueDate = new DateTime(2026, 7, 3),
            Priority = Priority.Low,
            Status = Status.New
        };

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(task));
        Assert.Contains("holiday", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

[Fact]
    public async Task Create_ShouldFail_WhenHighPriorityCapIsExceeded()
    {
        var service = new TaskService(_repo, new FakeHolidayProvider(), _clock);
        var due = new DateTime(2025, 12, 17); // Wednesday

        // Seed 100 High priority tasks with same due date and not finished
        for (var i = 0; i < 100; i++)
        {
            await _repo.AddAsync(new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = $"Seed-{i}",
                DueDate = due,
                StartDate = _clock.Today,
                Priority = Priority.High,
                Status = Status.InProgress
            });
        }

        var candidate = new TaskItem
        {
            Name = "Overflow",
            DueDate = due,
            StartDate = _clock.Today,
            Priority = Priority.High,
            Status = Status.New
        };

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(candidate));
        Assert.Contains("more than 100", ex.Message);
    }

    [Fact]
    public async Task Create_ShouldSucceed_ForValidTask()
    {
        var service = new TaskService(_repo, new FakeHolidayProvider(), _clock);
        var due = new DateTime(2025, 12, 17);

        var task = new TaskItem
        {
            Name = "Valid",
            Description = "Ok",
            StartDate = _clock.Today,
            DueDate = due,
            Priority = Priority.Medium,
            Status = Status.New
        };

        var created = await service.CreateAsync(task);
        Assert.NotEqual(Guid.Empty, created.Id);

        var fromRepo = await _repo.GetByIdAsync(created.Id);
        Assert.NotNull(fromRepo);
        Assert.Equal("Valid", fromRepo!.Name);
    }

    [Fact]
    public async Task Update_ShouldFail_WhenTaskDoesNotExist()
    {
        var service = new TaskService(_repo, new FakeHolidayProvider(), _clock);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdateAsync(Guid.NewGuid(), new TaskItem
            {
                Name = "Nope",
                DueDate = new DateTime(2025, 12, 17),
                StartDate = _clock.Today,
                Priority = Priority.Low,
                Status = Status.New
            }));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task Update_ShouldEnforce_HighPriorityCap_ExcludingSelf()
    {
        var service = new TaskService(_repo, new FakeHolidayProvider(), _clock);
        var due = new DateTime(2025, 12, 17);

        // Create the task we will update
        var id = Guid.NewGuid();
        await _repo.AddAsync(new TaskItem
        {
            Id = id,
            Name = "Mine",
            DueDate = due,
            StartDate = _clock.Today,
            Priority = Priority.High,
            Status = Status.InProgress
        });

        // Add 99 more (total 100 including 'Mine')
        for (var i = 0; i < 99; i++)
        {
            await _repo.AddAsync(new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = $"Seed-{i}",
                DueDate = due,
                StartDate = _clock.Today,
                Priority = Priority.High,
                Status = Status.InProgress
            });
        }

        // Updating the same task should be allowed (cap is checked excluding itself)
        var updated = await service.UpdateAsync(id, new TaskItem
        {
            Name = "Mine-Updated",
            DueDate = due,
            StartDate = _clock.Today,
            Priority = Priority.High,
            Status = Status.InProgress
        });

        Assert.Equal("Mine-Updated", updated.Name);
    }
}
