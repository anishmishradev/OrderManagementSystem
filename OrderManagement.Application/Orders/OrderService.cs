
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Interfaces.Repositories;
using OrderManagement.Application.Interfaces.Services;
using OrderManagement.Application.Orders.DTOs;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Orders;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<OrderResponse> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentException("Order request is required.");

        _logger.LogInformation(
            "Creating order. CustomerId: {CustomerId}, ItemCount: {ItemCount}",
            request.CustomerId,
            request.Items?.Count ?? 0);

        if (request.CustomerId <= 0 ||
            request.Items == null ||
            request.Items.Count == 0)
        {
            _logger.LogWarning(
                "Invalid order request. CustomerId: {CustomerId}",
                request.CustomerId);

            throw new ArgumentException(
                "Customer and order items are required.");
        }

        if (request.Items.Any(x => x.ProductId <= 0 || x.Quantity <= 0))
        {
            _logger.LogWarning(
                "Invalid product or quantity. CustomerId: {CustomerId}",
                request.CustomerId);

            throw new ArgumentException("Invalid product or quantity.");
        }

        if (request.Items.GroupBy(x => x.ProductId)
            .Any(x => x.Count() > 1))
        {
            _logger.LogWarning(
                "Duplicate products found. CustomerId: {CustomerId}",
                request.CustomerId);

            throw new ArgumentException(
                "Duplicate products are not allowed.");
        }

        if (!await _repository.CustomerExistsAsync(
            request.CustomerId, cancellationToken))
        {
            _logger.LogWarning(
                "Customer not found. CustomerId: {CustomerId}",
                request.CustomerId);

            throw new KeyNotFoundException("Customer not found.");
        }

        var productIds = request.Items
            .Select(x => x.ProductId)
            .ToList();

        var products = await _repository.GetProductsAsync(
            productIds, cancellationToken);

        if (products.Count != productIds.Count)
        {
            _logger.LogWarning(
                "One or more products not found. CustomerId: {CustomerId}",
                request.CustomerId);

            throw new KeyNotFoundException(
                "One or more products not found.");
        }

        var productLookup = products.ToDictionary(x => x.Id);

        var order = new Order
        {
            CustomerId = request.CustomerId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending
        };

        foreach (var item in request.Items)
        {
            var product = productLookup[item.ProductId];

            if (product.StockQuantity < item.Quantity)
            {
                _logger.LogWarning(
                    "Insufficient stock. ProductId: {ProductId}, Requested: {Requested}, Available: {Available}",
                    product.Id,
                    item.Quantity,
                    product.StockQuantity);

                throw new InvalidOperationException(
                    $"Insufficient stock for product {product.Id}.");
            }

            order.OrderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        var created = await _repository.CreateAsync(
            order, cancellationToken);

        var items = created.OrderItems
            .Select(x => new OrderItemResponse(
                x.Id,
                x.ProductId,
                productLookup[x.ProductId].Name,
                x.Quantity,
                x.UnitPrice,
                x.Quantity * x.UnitPrice))
            .ToList();

        var totalAmount = items.Sum(x => x.TotalPrice);

        _logger.LogInformation(
            "Order created successfully. OrderId: {OrderId}, CustomerId: {CustomerId}, TotalAmount: {TotalAmount}, ItemCount: {ItemCount}",
            created.Id,
            created.CustomerId,
            totalAmount,
            items.Count);

        return new OrderResponse(
            created.Id,
            created.CustomerId,
            created.OrderDate,
            created.Status.ToString(),
            totalAmount,
            items);
    }

    public async Task<OrderDetailsResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Retrieving order details. OrderId: {OrderId}",
            id);

        var order = await _repository.GetByIdAsync(
            id, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning(
                "Order not found. OrderId: {OrderId}",
                id);

            return null;
        }

        var items = order.OrderItems
            .Select(x => new OrderItemResponse(
                x.Id,
                x.ProductId,
                x.Product.Name,
                x.Quantity,
                x.UnitPrice,
                x.Quantity * x.UnitPrice))
            .ToList();

        _logger.LogInformation(
            "Order details retrieved successfully. OrderId: {OrderId}, CustomerId: {CustomerId}, ItemCount: {ItemCount}",
            order.Id,
            order.CustomerId,
            items.Count);

        return new OrderDetailsResponse(
            order.Id,
            order.OrderDate,
            order.Status.ToString(),
            new OrderCustomerResponse(
                order.Customer.Id,
                order.Customer.FullName,
                order.Customer.Email),
            items,
            items.Sum(x => x.TotalPrice));
    }


    public async Task<OrderStatusResponse?> UpdateStatusAsync(
        int id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null || !Enum.IsDefined(request.Status))
            throw new ArgumentException("Invalid order status.");

        var order = await _repository.GetByIdAsync(id, cancellationToken);

        if (order == null)
            return null;

        var currentStatus = order.Status;
        var newStatus = request.Status;

        if (currentStatus == newStatus)
            return new OrderStatusResponse(id, newStatus.ToString());
            bool isValid = false;

            if (currentStatus == OrderStatus.Pending)
            {
                isValid = newStatus == OrderStatus.Confirmed ||
                        newStatus == OrderStatus.Cancelled;
            }
            else if (currentStatus == OrderStatus.Confirmed)
            {
                isValid = newStatus == OrderStatus.Shipped ||
                        newStatus == OrderStatus.Cancelled;
            }

          
        if (!isValid)
            throw new InvalidOperationException(
                $"Cannot change order status from {currentStatus} to {newStatus}.");

        var updated = await _repository.UpdateStatusAsync(id, currentStatus, newStatus, cancellationToken);

        if (!updated)
            throw new InvalidOperationException(
                "Order status was changed by another request.");

        _logger.LogInformation(
            "Order {OrderId} status changed from {OldStatus} to {NewStatus}",
            id, currentStatus, newStatus);

        return new OrderStatusResponse(id, newStatus.ToString());
    }
}
