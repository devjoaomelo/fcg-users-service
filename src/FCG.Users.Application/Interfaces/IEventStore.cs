using FCG.Users.Application.Events;

namespace FCG.Users.Application.Interfaces;

public interface IEventStore
{
    Task AppendAsync(Guid aggregateId, string type, object? data, CancellationToken ct = default);
    Task<IReadOnlyList<EventRecord>> ListByAggregateAsync(Guid aggregateId, CancellationToken ct = default);
}
