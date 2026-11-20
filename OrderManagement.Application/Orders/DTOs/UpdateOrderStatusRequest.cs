
using OrderManagement.Domain.Enums;
namespace OrderManagement.Application.Orders.DTOs;

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}