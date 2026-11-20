
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product> AddAsync(
        Product product,
        CancellationToken cancellationToken = default);

    Task<List<Product>> GetAllAsync(
        CancellationToken cancellationToken = default);

   Task<List<Product>> SearchByNameAsync(
    string name,
    CancellationToken cancellationToken = default);

    Task<bool> SKUExistsAsync(
        string sku,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateStockAsync(
        int id,
        int quantity,
        CancellationToken cancellationToken = default);
}
