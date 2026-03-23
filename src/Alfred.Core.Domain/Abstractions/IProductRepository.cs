using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Domain.Abstractions;

public interface IProductRepository : IRepository<Product, ProductId>
{
}
