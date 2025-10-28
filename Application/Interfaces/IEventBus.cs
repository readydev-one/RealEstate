// RealEstate.Application/Interfaces/IEventBus.cs
using RealEstate.Domain.Events;

namespace RealEstate.Application.Interfaces;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent;
    void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent;
}