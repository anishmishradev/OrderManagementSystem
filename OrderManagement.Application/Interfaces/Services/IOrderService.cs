using OrderManagement.Application.Orders.DTOs;

namespace OrderManagement.Application.Interfaces.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<OrderDetailsResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<OrderStatusResponse?> UpdateStatusAsync(
        int id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default);
  
}