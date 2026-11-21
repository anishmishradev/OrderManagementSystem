
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OrderManagement.Application.Common;
using OrderManagement.Application.Interfaces.Services;
using OrderManagement.Application.Orders.DTOs;

namespace OrderManagement.Api.Controllers;

[Authorize]
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

    // CREATE ORDER 
    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.CreateAsync(
            request, cancellationToken);

        _logger.LogInformation(
            "Order {OrderId} created successfully",
            result.Id);

        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = result.Id },
            new ApiResponse<OrderResponse>
            {
                Success = true,
                StatusCode = StatusCodes.Status201Created,
                Message = "Order created successfully",
                Data = result
            });
    }

    // GET ORDER BY ID 
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrderById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.GetByIdAsync(
            id, cancellationToken);

        if (result == null)
        {
            _logger.LogWarning(
                "Order {OrderId} not found", id);

            return NotFound(new ApiResponse<OrderDetailsResponse>
            {
                Success = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Order not found"
            });
        }

        return Ok(new ApiResponse<OrderDetailsResponse>
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK,
            Message = "Order retrieved successfully",
            Data = result
        });
    }

    // UPDATE ORDER STATUS 
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateOrderStatus(
        int id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.UpdateStatusAsync(
            id, request, cancellationToken);

        if (result == null)
        {
            _logger.LogWarning(
                "Order {OrderId} not found", id);

            return NotFound(new ApiResponse<OrderStatusResponse>
            {
                Success = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Order not found"
            });
        }

        _logger.LogInformation(
            "Order {OrderId} status updated to {Status}",
            id, result.Status);

        return Ok(new ApiResponse<OrderStatusResponse>
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK,
            Message = "Order status updated successfully",
            Data = result
        });
    }
}
