// RealEstate.Domain/Events/IDomainEvent.cs
namespace RealEstate.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}