
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Customers.DTOs;
using OrderManagement.Application.Interfaces.Repositories;
using OrderManagement.Application.Interfaces.Services;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Customers;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository customerRepository,
        ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    // Create customer
    public async Task<CustomerResponse> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating customer");

        var email = request.Email.Trim();

        var emailExists = await _customerRepository.EmailExistsAsync(
            email, cancellationToken);

        if (emailExists)
        {
            _logger.LogWarning("Customer email already exists");

            throw new InvalidOperationException(
                "Customer with this email already exists.");
        }

        var customer = new Customer
        {
            FullName = request.FullName.Trim(),
            Email = email,
            CreatedDate = DateTime.UtcNow
        };

        var result = await _customerRepository.AddAsync(
            customer, cancellationToken);

        _logger.LogInformation(
            "Customer created successfully. CustomerId: {CustomerId}",
            result.Id);

        return new CustomerResponse(
            result.Id,
            result.FullName,
            result.Email,
            result.CreatedDate);
    }

    // Get customer by ID
    public async Task<CustomerResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Fetching customer. CustomerId: {CustomerId}", id);

        var customer = await _customerRepository.GetByIdAsync(
            id, cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning(
                "Customer not found. CustomerId: {CustomerId}", id);

            return null;
        }

        return new CustomerResponse(
            customer.Id,
            customer.FullName,
            customer.Email,
            customer.CreatedDate);
    }

    // Get all customers
    public async Task<List<CustomerResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all customers");

        var customers = await _customerRepository.GetAllAsync(
            cancellationToken);

        return customers.Select(customer => new CustomerResponse(
            customer.Id,
            customer.FullName,
            customer.Email,
            customer.CreatedDate
        )).ToList();
    }
}
