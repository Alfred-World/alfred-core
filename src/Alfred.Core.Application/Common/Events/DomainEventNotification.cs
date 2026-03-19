using Alfred.Core.Domain.Common.Events;

using MediatR;

namespace Alfred.Core.Application.Common.Events;

public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
