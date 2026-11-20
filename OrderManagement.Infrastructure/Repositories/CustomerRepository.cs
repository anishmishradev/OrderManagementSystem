
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Interfaces.Repositories;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public  class CustomerRepository : ICustomerRepository
{
    private readonly OrderManagementDbContext _context;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(
        OrderManagementDbContext context,
        ILogger<CustomerRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Create customer
    public async Task<Customer> AddAsync(
        Customer customer,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new customer");

        await _context.Customers.AddAsync(
            customer, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Customer created successfully. CustomerId: {CustomerId}",
            customer.Id);

        return customer;
    }

    // Get customer by ID
    public async Task<Customer?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Fetching customer. CustomerId: {CustomerId}", id);

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning(
                "Customer not found. CustomerId: {CustomerId}", id);
        }
        else
        {
            _logger.LogInformation(
                "Customer retrieved successfully. CustomerId: {CustomerId}",
                id);
        }

        return customer;
    }

    // Get all customers
    public async Task<List<Customer>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all customers");

        var customers = await _context.Customers
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Total customers retrieved: {Count}",
            customers.Count);

        return customers;
    }

    // Check duplicate email
    public async Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.Customers
            .AsNoTracking()
            .AnyAsync(
                x => x.Email == email,
                cancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Customer creation validation failed: duplicate email");
        }

        return exists;
    }
}
