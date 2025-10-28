// RealEstate.Domain/Events/PropertyUpdatedEvent.cs
namespace RealEstate.Domain.Events;

public class PropertyUpdatedEvent : IDomainEvent
{
    public string PropertyId { get; set; } = string.Empty;
    public bool RequiresClosingCostsRecalculation { get; set; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}