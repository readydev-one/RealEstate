// RealEstate.Domain/Events/CloserApprovalRequestedEvent.cs
namespace RealEstate.Domain.Events;

public class CloserApprovalRequestedEvent : IDomainEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}