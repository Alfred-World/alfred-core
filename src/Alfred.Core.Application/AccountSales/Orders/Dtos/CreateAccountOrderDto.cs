namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record CreateAccountOrderDto(
    MemberId MemberId,
    ProductId ProductId,
    ProductVariantId ProductVariantId,
    AccountCloneId AccountCloneId,
    MemberId? ReferrerMemberId,
    string? OrderNote,
    bool IsTrial = false
);
