namespace BookingHotel.Domain.Common;

public interface IDomainEvent
{
  string EventType { get; }

  DateTime OccurredOnUtc { get; }
}

public interface IAggregateRoot
{
  IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

  void ClearDomainEvents();
}

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
  private readonly List<IDomainEvent> _domainEvents = [];

  public IReadOnlyCollection<IDomainEvent> DomainEvents =>
      _domainEvents.AsReadOnly();

  protected void RaiseDomainEvent(IDomainEvent domainEvent)
  {
    _domainEvents.Add(domainEvent);
  }

  public void ClearDomainEvents()
  {
    _domainEvents.Clear();
  }
}
