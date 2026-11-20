namespace OrderManagement.Application.Orders.DTOs;

public record OrderItemResponse(
    int Id,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);

public record OrderResponse(
    int Id,
    int CustomerId,
    DateTime OrderDate,
    string Status,
    decimal TotalAmount,
    List<OrderItemResponse> Items
);

public record OrderCustomerResponse(
    int Id,
    string Name,
    string Email
);

public record OrderDetailsResponse(
    int Id,
    DateTime OrderDate,
    string Status,
    OrderCustomerResponse Customer,
    List<OrderItemResponse> Items,
    decimal TotalAmount
);

public record OrderStatusResponse(
    int Id,
    string Status
);