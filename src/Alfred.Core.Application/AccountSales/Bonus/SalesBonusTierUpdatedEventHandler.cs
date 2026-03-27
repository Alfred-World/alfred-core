using Alfred.Core.Application.Common;
using Alfred.Core.Application.Common.Events;
using Alfred.Core.Domain.Common.Events;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

using MediatR;

namespace Alfred.Core.Application.AccountSales.Bonus;

/// <summary>
/// Syncs snapshot values on all Pending bonus transactions when a tier's threshold or amount changes.
/// Runs after SaveChangesAsync so the main update flow is not blocked.
/// Paid/Cancelled transactions are never touched — their snapshots are frozen at trigger time.
/// </summary>
public sealed class SalesBonusTierUpdatedEventHandler : INotificationHandler<DomainEventNotification>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAsyncQueryExecutor _executor;

    public SalesBonusTierUpdatedEventHandler(IUnitOfWork unitOfWork, IAsyncQueryExecutor executor)
    {
        _unitOfWork = unitOfWork;
        _executor = executor;
    }

    public async Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
    {
        if (notification.DomainEvent is not SalesBonusTierUpdatedDomainEvent e)
        {
            return;
        }

        var pendingTxs = await _executor.ToListAsync(
            _unitOfWork.SalesBonusTransactions.GetQueryable()
                .Where(t => t.SalesBonusTierId == e.TierId && t.Status == SalesBonusTransactionStatus.Pending),
            cancellationToken);

        if (pendingTxs.Count == 0)
        {
            return;
        }

        foreach (var tx in pendingTxs)
        {
            var bonusDelta = e.NewBonusAmount - tx.BonusAmountSnapshot;

            tx.UpdatePendingSnapshot(e.NewOrderThreshold, e.NewBonusAmount);
            _unitOfWork.SalesBonusTransactions.Update(tx);

            if (bonusDelta != 0m)
            {
                var summary = await _unitOfWork.MemberMonthlySalesSummaries
                    .GetBySellerAndPeriodAsync(tx.SoldByMemberId, tx.Year, tx.Month, cancellationToken);
                summary?.AdjustTotalBonusEarned(bonusDelta);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
