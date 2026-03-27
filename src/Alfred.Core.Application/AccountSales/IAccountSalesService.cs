using Alfred.Core.Application.AccountSales.AccountClones;
using Alfred.Core.Application.AccountSales.Bonus;
using Alfred.Core.Application.AccountSales.Commission;
using Alfred.Core.Application.AccountSales.Members;
using Alfred.Core.Application.AccountSales.Orders;
using Alfred.Core.Application.AccountSales.Products;
using Alfred.Core.Application.AccountSales.Warranty;

namespace Alfred.Core.Application.AccountSales;

public interface IAccountSalesService
    : IProductService,
        IMemberService,
        IAccountCloneService,
        IOrderService,
        ICommissionService,
        IBonusService,
        IWarrantyService
{
}
