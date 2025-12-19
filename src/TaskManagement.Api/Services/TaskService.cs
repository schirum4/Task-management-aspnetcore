using TaskManagement.Api.Domain;
using TaskManagement.Api.Infrastructure;
using TaskManagement.Api.Repositories;

namespace TaskManagement.Api.Services;

public sealed class TaskService : ITaskService
{
    private readonly ITaskRepository _repo;
    private readonly IHolidayProvider _holidays;
    private readonly IClock _clock;

    public TaskService(ITaskRepository repo, IHolidayProvider holidays, IClock clock)
    {
        _repo = repo;
        _holidays = holidays;
        _clock = clock;
    }

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default)
    {
        Normalize(task);
        ValidateRequired(task);
        ValidateDueDate(task.DueDate);

        await EnforceHighPriorityCapAsync(task, excludingId: null, ct);

        await _repo.AddAsync(task, ct);
        return task;
    }

    public async Task<TaskItem> UpdateAsync(Guid id, TaskItem task, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null)
            throw new KeyNotFoundException($"Task '{id}' not found.");

        task.Id = id;
        Normalize(task);
        ValidateRequired(task);
        ValidateDueDate(task.DueDate);

        await EnforceHighPriorityCapAsync(task, excludingId: id, ct);

        await _repo.UpdateAsync(task, ct);
        return task;
    }

    private void Normalize(TaskItem task)
    {
        task.Name = task.Name?.Trim() ?? string.Empty;
        task.Description = task.Description?.Trim() ?? string.Empty;
    }

    private static void ValidateRequired(TaskItem task)
    {
        if (string.IsNullOrWhiteSpace(task.Name))
            throw new BusinessRuleException("Name is required.");

        if (task.StartDate == default)
            task.StartDate = DateTime.Today;

        // If Finished, EndDate should exist (not required by prompt, but helpful)
        if (task.Status == Status.Finished && task.EndDate is null)
            task.EndDate = DateTime.Today;
    }

    private void ValidateDueDate(DateTime dueDate)
    {
        var due = dueDate.Date;
        var today = _clock.Today.Date;

        if (due < today)
            throw new BusinessRuleException("Due Date cannot be in the past.");

        if (due.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            throw new BusinessRuleException("Due Date cannot be on a weekend.");

        if (_holidays.IsHoliday(due))
            throw new BusinessRuleException("Due Date cannot be on a holiday.");
    }

    private async Task EnforceHighPriorityCapAsync(TaskItem candidate, Guid? excludingId, CancellationToken ct)
    {
        // Only applies to High priority tasks that are not finished
        if (candidate.Priority != Priority.High || candidate.Status == Status.Finished)
            return;

        var all = await _repo.GetAllAsync(ct);

        var count = all.Count(t =>
            t.Priority == Priority.High &&
            t.Status != Status.Finished &&
            t.DueDate.Date == candidate.DueDate.Date &&
            (excludingId is null || t.Id != excludingId.Value));

        if (count >= 100)
        {
            throw new BusinessRuleException(
                "Cannot have more than 100 High Priority tasks with the same due date that are not finished.");
        }
    }
}
