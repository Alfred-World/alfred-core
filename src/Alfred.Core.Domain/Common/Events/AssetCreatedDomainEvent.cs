namespace Alfred.Core.Domain.Common.Events;

public sealed record AssetCreatedDomainEvent(AssetId AssetId) : DomainEvent;
