using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<bool> CustomerExistsAsync(
        int customerId,
        CancellationToken cancellationToken = default);

    Task<List<Product>> GetProductsAsync(
        List<int> productIds,
        CancellationToken cancellationToken = default);

    Task<Order> CreateAsync(
        Order order,
        CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateStatusAsync(
        int id,
        OrderStatus currentStatus,
        OrderStatus newStatus,
        CancellationToken cancellationToken = default);


}