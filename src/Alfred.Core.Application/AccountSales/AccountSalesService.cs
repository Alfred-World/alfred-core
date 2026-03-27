using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.Common;
using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccountSales;

public sealed partial class AccountSalesService : BaseApplicationService, IAccountSalesService
{
    private readonly IUnitOfWork _unitOfWork;

    public AccountSalesService(
        IUnitOfWork unitOfWork,
        IFilterParser filterParser,
        IAsyncQueryExecutor executor) : base(filterParser, executor)
    {
        _unitOfWork = unitOfWork;
    }

    private static IReadOnlyList<CreateProductVariantDto> NormalizeCreateVariants(
        IReadOnlyList<CreateProductVariantDto> variants)
    {
        if (variants.Count == 0)
        {
            throw new InvalidOperationException("At least one product variant is required.");
        }

        var normalized = variants
            .Select(x => new CreateProductVariantDto(
                x.Name.Trim(),
                Math.Max(0m, decimal.Round(x.Price, 2, MidpointRounding.AwayFromZero)),
                Math.Max(0, x.WarrantyDays)))
            .ToList();

        if (normalized.Any(x => string.IsNullOrWhiteSpace(x.Name)))
        {
            throw new InvalidOperationException("Product variant name is required.");
        }

        var duplicateName = normalized
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(x => x.Count() > 1);

        if (duplicateName is not null)
        {
            throw new InvalidOperationException($"Duplicate variant name '{duplicateName.Key}'.");
        }

        return normalized;
    }

    private static IReadOnlyList<UpdateProductVariantDto> NormalizeUpdateVariants(
        IReadOnlyList<UpdateProductVariantDto> variants)
    {
        if (variants.Count == 0)
        {
            throw new InvalidOperationException("At least one product variant is required.");
        }

        var normalized = variants
            .Select(x => new UpdateProductVariantDto(
                x.Name.Trim(),
                Math.Max(0m, decimal.Round(x.Price, 2, MidpointRounding.AwayFromZero)),
                Math.Max(0, x.WarrantyDays)))
            .ToList();

        if (normalized.Any(x => string.IsNullOrWhiteSpace(x.Name)))
        {
            throw new InvalidOperationException("Product variant name is required.");
        }

        var duplicateName = normalized
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(x => x.Count() > 1);

        if (duplicateName is not null)
        {
            throw new InvalidOperationException($"Duplicate variant name '{duplicateName.Key}'.");
        }

        return normalized;
    }

    private async Task<Dictionary<Guid, ReplicatedSellerSnapshot>> GetReplicatedSellerMapAsync(
        IEnumerable<Guid?> sellerIds,
        CancellationToken cancellationToken)
    {
        var ids = sellerIds
            .Where(x => x.HasValue && x.Value != default)
            .Select(x => (ReplicatedUserId)x!.Value)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<Guid, ReplicatedSellerSnapshot>();
        }

        var users = await _executor.ToListAsync(
            _unitOfWork.ReplicatedUsers
                .GetQueryable()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new ReplicatedSellerSnapshot(x.Id, x.Email, x.FullName, x.Avatar)),
            cancellationToken);

        return users.ToDictionary(x => (Guid)x.Id, x => x);
    }

    private static ReplicatedUserDto ToReplicatedUserDto(ReplicatedSellerSnapshot seller)
    {
        return new ReplicatedUserDto
        {
            Id = seller.Id,
            UserName = null,
            Email = seller.Email,
            FullName = seller.FullName,
            Avatar = seller.Avatar
        };
    }

    private static ReplicatedUserDto ToReplicatedUserDto(ReplicatedUser user)
    {
        return new ReplicatedUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            Avatar = user.Avatar
        };
    }

    private sealed record ReplicatedSellerSnapshot(
        ReplicatedUserId Id,
        string Email,
        string? FullName,
        string? Avatar);
}
