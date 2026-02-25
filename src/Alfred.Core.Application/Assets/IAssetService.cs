using Alfred.Core.Application.Assets.Dtos;
using Alfred.Core.Application.Querying.Core;

namespace Alfred.Core.Application.Assets;

public interface IAssetService
{
    #region Assets

    Task<PageResult<AssetDto>> GetAllAssetsAsync(QueryRequest query,
        CancellationToken cancellationToken = default);

    Task<AssetDto?> GetAssetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AssetDto> CreateAssetAsync(CreateAssetDto dto, CancellationToken cancellationToken = default);

    Task<AssetDto> UpdateAssetAsync(Guid id, UpdateAssetDto dto, CancellationToken cancellationToken = default);

    Task DeleteAssetAsync(Guid id, CancellationToken cancellationToken = default);

    #endregion

    #region Asset Logs

    Task<PageResult<AssetLogDto>> GetAssetLogsAsync(Guid assetId, QueryRequest query,
        CancellationToken cancellationToken = default);

    Task<AssetLogDto?> GetAssetLogByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AssetLogDto> CreateAssetLogAsync(Guid assetId, CreateAssetLogDto dto,
        CancellationToken cancellationToken = default);

    Task DeleteAssetLogAsync(Guid id, CancellationToken cancellationToken = default);

    #endregion
}
