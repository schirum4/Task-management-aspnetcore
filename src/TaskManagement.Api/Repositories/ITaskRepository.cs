using TaskManagement.Api.Domain;

namespace TaskManagement.Api.Repositories;

public interface ITaskRepository
{
    Task AddAsync(TaskItem task, CancellationToken ct = default);
    Task UpdateAsync(TaskItem task, CancellationToken ct = default);
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken ct = default);
}
