using System.Collections.Concurrent;
using TaskManagement.Api.Domain;

namespace TaskManagement.Api.Repositories;

/// <summary>
/// Thread-safe in-memory repository (no DB required for this assignment).
/// </summary>
public sealed class InMemoryTaskRepository : ITaskRepository
{
    private readonly ConcurrentDictionary<Guid, TaskItem> _store = new();

    public Task AddAsync(TaskItem task, CancellationToken ct = default)
    {
        _store[task.Id] = Clone(task);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        _store[task.Id] = Clone(task);
        return Task.CompletedTask;
    }

    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(_store.TryGetValue(id, out var task) ? Clone(task) : null);
    }

    public Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken ct = default)
    {
        var list = _store.Values.Select(Clone).ToList().AsReadOnly();
        return Task.FromResult((IReadOnlyList<TaskItem>)list);
    }

    private static TaskItem Clone(TaskItem t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Description = t.Description,
        StartDate = t.StartDate,
        DueDate = t.DueDate,
        EndDate = t.EndDate,
        Priority = t.Priority,
        Status = t.Status
    };
}
