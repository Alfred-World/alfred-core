namespace Alfred.Core.Domain.Common.Events;

public sealed record AssetUpdatedDomainEvent(AssetId AssetId) : DomainEvent;
