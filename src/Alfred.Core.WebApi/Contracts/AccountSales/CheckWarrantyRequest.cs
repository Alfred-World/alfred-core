using Alfred.Core.Application.AccountSales.Dtos;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record CheckWarrantyRequest
{
    public Guid? ProductId { get; init; }
    public string? Username { get; init; }

    public CheckWarrantyDto ToDto()
    {
        return new CheckWarrantyDto((ProductId?)ProductId, Username);
    }
}
