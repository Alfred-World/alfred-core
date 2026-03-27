using Alfred.Core.Application.AccountSales.Dtos;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record ConfirmPaymentRequest
{
    public Guid OrderId { get; init; }

    public ConfirmPaymentDto ToDto()
    {
        return new ConfirmPaymentDto((AccountOrderId)OrderId);
    }
}

public sealed record RefundOrderRequest
{
    public Guid OrderId { get; init; }
    public decimal RefundAmount { get; init; }
    public string? Note { get; init; }

    public RefundOrderDto ToDto()
    {
        return new RefundOrderDto((AccountOrderId)OrderId, RefundAmount, Note);
    }
}

public sealed record PayoutCommissionRequest
{
    public Guid MemberId { get; init; }
    public string? EvidenceObjectKey { get; init; }
    public string? Note { get; init; }

    public PayoutCommissionDto ToDto()
    {
        return new PayoutCommissionDto((MemberId)MemberId, EvidenceObjectKey, Note);
    }
}
