using Alfred.Core.Application.Assets.Dtos;
using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.Assets;

public interface IAssetService
{
    #region Assets

    Task<PageResult<AssetDto>> SearchAssetsAsync(SearchRequest request,
        CancellationToken cancellationToken = default);

    Task<AssetDto?> GetAssetByIdAsync(AssetId id, CancellationToken cancellationToken = default);

    Task<AssetDto> CreateAssetAsync(CreateAssetDto dto, CancellationToken cancellationToken = default);

    Task<AssetDto> UpdateAssetAsync(AssetId id, UpdateAssetDto dto, CancellationToken cancellationToken = default);

    Task DeleteAssetAsync(AssetId id, CancellationToken cancellationToken = default);

    #endregion

    #region Asset Logs

    Task<PageResult<AssetLogDto>> SearchAssetLogsAsync(AssetId assetId, SearchRequest request,
        CancellationToken cancellationToken = default);

    Task<AssetLogDto?> GetAssetLogByIdAsync(AssetLogId id, CancellationToken cancellationToken = default);

    Task<AssetLogDto> CreateAssetLogAsync(AssetId assetId, CreateAssetLogDto dto,
        CancellationToken cancellationToken = default);

    Task DeleteAssetLogAsync(AssetLogId id, CancellationToken cancellationToken = default);

    #endregion
}
