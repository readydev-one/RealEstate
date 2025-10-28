// RealEstate.Domain/Events/DownPaymentAcceptedEvent.cs
namespace RealEstate.Domain.Events;

public class DownPaymentAcceptedEvent : IDomainEvent
{
    public string PropertyId { get; set; } = string.Empty;
    public List<string> LockedBuyerIds { get; set; } = new();
    public decimal Amount { get; set; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}