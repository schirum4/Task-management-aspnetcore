using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.Contracts;
using TaskManagement.Api.Domain;
using TaskManagement.Api.Repositories;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _service;
    private readonly ITaskRepository _repo;

    public TasksController(ITaskService service, ITaskRepository repo)
    {
        _service = service;
        _repo = repo;
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create([FromBody] UpsertTaskRequest request, CancellationToken ct)
    {
        try
        {
            var task = MapToDomain(request);
            var created = await _service.CreateAsync(task, ct);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Not required by the prompt, but useful for CreatedAtAction and quick manual testing.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var task = await _repo.GetByIdAsync(id, ct);
        if (task is null)
            return NotFound(new { error = "Task not found." });

        return Ok(MapToResponse(task));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> Update([FromRoute] Guid id, [FromBody] UpsertTaskRequest request, CancellationToken ct)
    {
        try
        {
            var task = MapToDomain(request);
            var updated = await _service.UpdateAsync(id, task, ct);
            return Ok(MapToResponse(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Task not found." });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static TaskItem MapToDomain(UpsertTaskRequest r) => new()
    {
        Name = r.Name,
        Description = r.Description,
        DueDate = r.DueDate,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        Priority = r.Priority,
        Status = r.Status
    };

    private static TaskResponse MapToResponse(TaskItem t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Description = t.Description,
        DueDate = t.DueDate,
        StartDate = t.StartDate,
        EndDate = t.EndDate,
        Priority = t.Priority,
        Status = t.Status
    };
}
