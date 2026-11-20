
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Interfaces.Repositories;

public interface ICustomerRepository
{
    // Create customer
    Task<Customer> AddAsync(
        Customer customer,
        CancellationToken cancellationToken = default);

    // Get customer by ID
    Task<Customer?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    // Get all customers
    Task<List<Customer>> GetAllAsync(
        CancellationToken cancellationToken = default);

    // Check whether email already exists
    Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default);
}
