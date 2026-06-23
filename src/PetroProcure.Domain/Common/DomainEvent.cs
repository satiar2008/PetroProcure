namespace PetroProcure.Domain.Common;

public abstract record DomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();

    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
