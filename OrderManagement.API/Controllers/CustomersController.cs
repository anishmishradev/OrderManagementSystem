
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Customers.DTOs;
using OrderManagement.Application.Interfaces.Services;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICustomerService customerService,
        ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    // POST: api/customers
    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> Create(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create customer request received");

        var customer = await _customerService.CreateAsync(
            request, cancellationToken);

        _logger.LogInformation(
            "Customer created. CustomerId: {CustomerId}",
            customer.Id);

        return CreatedAtAction(
            nameof(GetById),
            new { id = customer.Id },
            customer);
    }

    // GET: api/customers/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(
            id, cancellationToken);

        if (customer == null)
        {
            return NotFound(new
            {
                Message = "Customer not found"
            });
        }

        return Ok(customer);
    }

    // GET: api/customers
    [HttpGet]
    public async Task<ActionResult<List<CustomerResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetAllAsync(
            cancellationToken);

        return Ok(customers);
    }
}
