using OrderManagement.Application.Customers.DTOs;

namespace OrderManagement.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<CustomerResponse> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<CustomerResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<List<CustomerResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);
}