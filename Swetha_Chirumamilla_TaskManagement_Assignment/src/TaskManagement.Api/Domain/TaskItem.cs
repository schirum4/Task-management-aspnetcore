namespace TaskManagement.Api.Domain;

public enum Priority
{
    Low,
    Medium,
    High
}

public enum Status
{
    New,
    InProgress,
    Finished
}

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? EndDate { get; set; }

    public Priority Priority { get; set; }
    public Status Status { get; set; }
}
