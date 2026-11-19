namespace OrderManagement.Application.Customers;
/// <summary>
/// Customer Response
/// </summary>
/// <param name="Id"></param>
/// <param name="FullName"></param>
/// <param name="Email"></param>
/// <param name="CreatedDate"></param>
public record CustomerResponse(
    int Id,
    string FullName,
    string Email,
    DateTime CreatedDate
);