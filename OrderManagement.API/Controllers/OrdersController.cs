
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Common;
using OrderManagement.Application.Interfaces.Services;
using OrderManagement.Application.Orders.DTOs;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderService orderService,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.CreateAsync(request, cancellationToken);

        _logger.LogInformation("Order {OrderId} created successfully", result.Id);

        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = result.Id },
            new ApiResponse<OrderResponse>
            {
                Success = true,
                Message = "Order created successfully",
                Data = result
            });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrderById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.GetByIdAsync(id, cancellationToken);

        if (result == null)
            return NotFound(new ApiResponse<OrderDetailsResponse>
            {
                Success = false,
                Message = "Order not found"
            });

        return Ok(new ApiResponse<OrderDetailsResponse>
        {
            Success = true,
            Message = "Order retrieved successfully",
            Data = result
        });
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateOrderStatus(
        int id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.UpdateStatusAsync(
            id, request, cancellationToken);

        if (result == null)
            return NotFound(new ApiResponse<OrderStatusResponse>
            {
                Success = false,
                Message = "Order not found"
            });

        _logger.LogInformation(
            "Order {OrderId} status updated to {Status}",
            id, result.Status);

        return Ok(new ApiResponse<OrderStatusResponse>
        {
            Success = true,
            Message = "Order status updated successfully",
            Data = result
        });
    }

}
