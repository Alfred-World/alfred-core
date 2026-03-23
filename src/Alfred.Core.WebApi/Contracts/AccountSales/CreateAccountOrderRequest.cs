using Alfred.Core.Application.AccountSales.Dtos;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record CreateAccountOrderRequest
{
    public Guid MemberId { get; init; }
    public Guid ProductId { get; init; }
    public Guid ProductVariantId { get; init; }
    public Guid AccountCloneId { get; init; }
    public Guid? ReferrerMemberId { get; init; }
    public string? OrderNote { get; init; }

    public CreateAccountOrderDto ToDto()
    {
        return new CreateAccountOrderDto(MemberId, ProductId, ProductVariantId, AccountCloneId, ReferrerMemberId,
            OrderNote);
    }
}
