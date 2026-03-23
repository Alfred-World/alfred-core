namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record CreateAccountOrderDto(
    Guid MemberId,
    Guid ProductId,
    Guid ProductVariantId,
    Guid AccountCloneId,
    Guid? ReferrerMemberId,
    string? OrderNote
);
