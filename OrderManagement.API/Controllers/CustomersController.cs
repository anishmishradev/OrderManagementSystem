
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Common;
using OrderManagement.Application.Customers.DTOs;
using OrderManagement.Application.Interfaces.Services;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
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

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> Create(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Create customer request received");

            var customer = await _customerService.CreateAsync(
                request, cancellationToken);

            _logger.LogInformation(
                "Customer created. CustomerId: {CustomerId}",
                customer.Id);

            var response = ApiResponse<CustomerResponse>.Ok(
                customer,
                "Customer created successfully",
                StatusCodes.Status201Created);

            return CreatedAtAction(
                nameof(GetById),
                new { id = customer.Id },
                response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid customer request");

            return BadRequest(
                ApiResponse<CustomerResponse>.Fail(
                    ex.Message,
                    StatusCodes.Status400BadRequest));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Customer creation conflict");

            return Conflict(
                ApiResponse<CustomerResponse>.Fail(
                    ex.Message,
                    StatusCodes.Status409Conflict));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(
                ApiResponse<CustomerResponse>.Fail(
                    "Invalid customer ID",
                    StatusCodes.Status400BadRequest));
        }

        var customer = await _customerService.GetByIdAsync(
            id, cancellationToken);

        if (customer is null)
        {
            return NotFound(
                ApiResponse<CustomerResponse>.Fail(
                    "Customer not found",
                    StatusCodes.Status404NotFound));
        }

        return Ok(
            ApiResponse<CustomerResponse>.Ok(
                customer,
                "Customer retrieved successfully"));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CustomerResponse>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetAllAsync(
            cancellationToken);

        return Ok(
            ApiResponse<List<CustomerResponse>>.Ok(
                customers,
                "Customers retrieved successfully"));
    }
}
