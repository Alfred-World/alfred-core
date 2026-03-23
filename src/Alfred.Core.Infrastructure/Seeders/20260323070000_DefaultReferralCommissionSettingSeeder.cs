using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Common.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Alfred.Core.Infrastructure.Seeders;

/// <summary>
/// Seeds a default referral commission setting when the table is empty.
/// </summary>
public sealed class DefaultReferralCommissionSettingSeeder : BaseDataSeeder
{
    private const decimal DefaultCommissionPercent = 5m;

    private readonly IDbContext _dbContext;

    public DefaultReferralCommissionSettingSeeder(
        IDbContext dbContext,
        ILogger<DefaultReferralCommissionSettingSeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override string Name => "20260323070000_DefaultReferralCommissionSettingSeeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var existingSetting = await _dbContext.ReferralCommissionSettings
            .AsNoTracking()
            .AnyAsync(cancellationToken);

        if (existingSetting)
        {
            LogSuccess("Referral commission setting already exists");
            return;
        }

        var changedAt = DateTime.UtcNow;
        var setting = ReferralCommissionSetting.Create(DefaultCommissionPercent);

        await _dbContext.ReferralCommissionSettings.AddAsync(setting, cancellationToken);

        var history = ReferralCommissionSettingHistory.Create(
            setting.Id,
            0m,
            DefaultCommissionPercent,
            null,
            changedAt);

        await _dbContext.ReferralCommissionSettingHistories.AddAsync(history, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogSuccess($"Created default referral commission setting: {DefaultCommissionPercent:0.##}%");
    }
}
