using MediatR;

// Should be used within the repository dispatching the events after persistence (saved). 

namespace _SharedKernel.Patterns.DomainRules;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public DomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DispatchEventsAsync<T>(T entity) where T : Entity
    {
        var domainEvents = entity.DomainEvents?.ToList() ?? new List<INotification>();

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent);
        }

        entity.ClearDomainEvents();
    }
}
