// RealEstate.Infrastructure/EventBus/InMemoryEventBus.cs
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Events;

namespace RealEstate.Infrastructure.EventBus;

public class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            foreach (var handler in handlers)
            {
                if (handler is Func<TEvent, Task> typedHandler)
                {
                    await typedHandler(@event);
                }
            }
        }
    }

    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Delegate>();
        }
        
        _handlers[eventType].Add(handler);
    }
}