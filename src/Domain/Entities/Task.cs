// RealEstate.Domain/Entities/Task.cs
using RealEstate.Domain.Enums;

namespace RealEstate.Domain.Entities;

public class TaskEntity : BaseEntity
{
    public string PropertyId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskType TaskType { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.NotStarted;
    public DateTime? CompletedAt { get; set; }
}