using TaskManagement.Api.Domain;

namespace TaskManagement.Api.Services;

public interface ITaskService
{
    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default);
    Task<TaskItem> UpdateAsync(Guid id, TaskItem task, CancellationToken ct = default);
}
