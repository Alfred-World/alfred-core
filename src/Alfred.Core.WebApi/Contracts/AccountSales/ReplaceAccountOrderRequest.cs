using Alfred.Core.Application.AccountSales.Dtos;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record ReplaceAccountOrderRequest
{
    public Guid ReplacementAccountCloneId { get; init; }
    public string? OrderNote { get; init; }
    public string? WarrantyIssueNote { get; init; }

    public ReplaceAccountOrderDto ToDto()
    {
        return new ReplaceAccountOrderDto((AccountCloneId)ReplacementAccountCloneId, OrderNote, WarrantyIssueNote);
    }
}
