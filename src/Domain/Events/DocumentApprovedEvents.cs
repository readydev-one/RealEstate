// RealEstate.Domain/Events/DocumentApprovedEvent.cs
namespace RealEstate.Domain.Events;

public class DocumentApprovedEvent : IDomainEvent
{
    public string DocumentId { get; set; } = string.Empty;
    public string PropertyId { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public List<string> VisibleTo { get; set; } = new();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}