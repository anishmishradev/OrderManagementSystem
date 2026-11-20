
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Interfaces.Repositories;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Data;
using System.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderManagementDbContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(
        OrderManagementDbContext context,
        ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<bool> CustomerExistsAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        return _context.Customers
            .AnyAsync(x => x.Id == customerId, cancellationToken);
    }

    public Task<List<Product>> GetProductsAsync(
        List<int> productIds,
        CancellationToken cancellationToken = default)
    {
        return _context.Products
            .AsNoTracking()
            .Where(x => productIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Order> CreateAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            // Deduct stock before saving the order.
            foreach (var item in order.OrderItems.OrderBy(x => x.ProductId))
            {
                var updated = await _context.Products
                    .Where(x => x.Id == item.ProductId &&
                                x.StockQuantity >= item.Quantity)
                    .ExecuteUpdateAsync(
                        x => x.SetProperty(
                            p => p.StockQuantity,
                            p => p.StockQuantity - item.Quantity),
                        cancellationToken);

                if (updated == 0)
                {
                    _logger.LogWarning(
                        "Insufficient stock for product {ProductId}",
                        item.ProductId);

                    throw new InvalidOperationException(
                        $"Insufficient stock for product {item.ProductId}");
                }
            }

            // Save the order and its items.
            await _context.Orders.AddAsync(order, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Order {OrderId} created for customer {CustomerId}",
                order.Id,
                order.CustomerId);

            return order;
        });
    }

    public Task<Order?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _context.Orders
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    
public async Task<bool> UpdateStatusAsync(
    int id,
    OrderStatus currentStatus,
    OrderStatus newStatus,
    CancellationToken cancellationToken = default)
{
    _logger.LogInformation(
        "Updating order status. OrderId: {OrderId}, From: {CurrentStatus}, To: {NewStatus}",
        id, currentStatus, newStatus);

    var strategy = _context.Database.CreateExecutionStrategy();

    return await strategy.ExecuteAsync(async () =>
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        // Prevent concurrent status updates.
        var updated = await _context.Orders
            .Where(x => x.Id == id && x.Status == currentStatus)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, newStatus),
                cancellationToken);

        if (updated == 0)
        {
            _logger.LogWarning(
                "Order status update failed. OrderId: {OrderId}, ExpectedStatus: {Status}",
                id, currentStatus);

            return false;
        }

        // Restore inventory when an order is cancelled.
        if (newStatus == OrderStatus.Cancelled)
        {
            var orderItems = await _context.OrderItems
                .AsNoTracking()
                .Where(x => x.OrderId == id)
                .ToListAsync(cancellationToken);

            foreach (var item in orderItems.OrderBy(x => x.ProductId))
            {
                var stockUpdated = await _context.Products
                    .Where(x => x.Id == item.ProductId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(
                            x => x.StockQuantity,
                            x => x.StockQuantity + item.Quantity),
                        cancellationToken);

                if (stockUpdated == 0)
                {
                    _logger.LogError(
                        "Inventory restoration failed. OrderId: {OrderId}, ProductId: {ProductId}",
                        id, item.ProductId);

                    throw new InvalidOperationException(
                        $"Product {item.ProductId} not found.");
                }

                _logger.LogInformation(
                    "Inventory restored. OrderId: {OrderId}, ProductId: {ProductId}, Quantity: {Quantity}",
                    id, item.ProductId, item.Quantity);
            }
        }

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Order status updated successfully. OrderId: {OrderId}, From: {OldStatus}, To: {NewStatus}",
            id, currentStatus, newStatus);

        return true;
    });
}

}
