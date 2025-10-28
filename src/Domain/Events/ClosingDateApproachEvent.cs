// RealEstate.Domain/Events/ClosingDateApproachingEvent.cs
namespace RealEstate.Domain.Events;

public class ClosingDateApproachingEvent : IDomainEvent
{
    public string PropertyId { get; set; } = string.Empty;
    public DateTime ClosingDate { get; set; }
    public List<string> NotifyUserIds { get; set; } = new();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
