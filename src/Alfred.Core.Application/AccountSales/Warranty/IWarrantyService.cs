using Alfred.Core.Application.AccountSales.Dtos;

namespace Alfred.Core.Application.AccountSales.Warranty;

public interface IWarrantyService
{
    Task<GithubUserProfileDto>
        GetGithubUserProfileAsync(string username, CancellationToken cancellationToken = default);

    Task<WarrantyCheckResultDto>
        CheckWarrantyAsync(CheckWarrantyDto dto, CancellationToken cancellationToken = default);
}
