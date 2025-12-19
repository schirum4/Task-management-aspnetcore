using System.ComponentModel.DataAnnotations;
using TaskManagement.Api.Domain;

namespace TaskManagement.Api.Contracts;

public sealed class UpsertTaskRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime DueDate { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    public Priority Priority { get; set; }

    [Required]
    public Status Status { get; set; }
}

public sealed class TaskResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Priority Priority { get; set; }
    public Status Status { get; set; }
}
