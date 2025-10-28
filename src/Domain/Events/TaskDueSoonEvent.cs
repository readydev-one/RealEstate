// RealEstate.Domain/Events/TaskDueSoonEvent.cs
namespace RealEstate.Domain.Events;

public class TaskDueSoonEvent : IDomainEvent
{
    public string TaskId { get; set; } = string.Empty;
    public string PropertyId { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}