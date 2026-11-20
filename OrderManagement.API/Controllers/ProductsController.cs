
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Common;
using OrderManagement.Application.Products.DTOs;
using OrderManagement.Application.Interfaces.Services;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Create product request received. SKU: {SKU}",
            request.SKU);

        var product = await _productService.CreateAsync(
            request, cancellationToken);

        _logger.LogInformation(
            "Product created successfully. ProductId: {ProductId}, SKU: {SKU}",
            product.Id,
            product.SKU);

        var response = ApiResponse<ProductResponse>.Ok(
            product,
            "Product created successfully",
            StatusCodes.Status201Created);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProductResponse>>>> GetAll(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get all products request received");

        var products = await _productService.GetAllAsync(
            cancellationToken);

        _logger.LogInformation(
            "Products retrieved successfully. TotalCount: {Count}",
            products.Count);

        return Ok(
            ApiResponse<List<ProductResponse>>.Ok(
                products,
                "Products retrieved successfully"));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<ProductResponse>>>> Search(
        [FromQuery] string? name,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 150)
        {
            _logger.LogWarning(
                "Invalid product search request");

            return BadRequest(
                ApiResponse<List<ProductResponse>>.Fail(
                    "Product name must contain between 1 and 150 characters.",
                    StatusCodes.Status400BadRequest));
        }

        _logger.LogInformation("Product search request received");

        var products = await _productService.SearchByNameAsync(
            name, cancellationToken);

        _logger.LogInformation(
            "Product search completed. ResultCount: {Count}",
            products.Count);

        return Ok(
            ApiResponse<List<ProductResponse>>.Ok(
                products,
                "Product search completed successfully"));
    }

    [HttpPatch("{id:int}/stock")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStock(
        int id,
        [FromBody] UpdateProductStockRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Update stock request received. ProductId: {ProductId}, NewQuantity: {Quantity}",
            id,
            request.StockQuantity);

        if (id <= 0 || request.StockQuantity < 0)
        {
            _logger.LogWarning(
                "Invalid stock update request. ProductId: {ProductId}",
                id);

            return BadRequest(
                ApiResponse<object>.Fail(
                    "Invalid product ID or stock quantity.",
                    StatusCodes.Status400BadRequest));
        }

        var updated = await _productService.UpdateStockAsync(
            id, request, cancellationToken);

        if (!updated)
        {
            _logger.LogWarning(
                "Product not found during stock update. ProductId: {ProductId}",
                id);

            return NotFound(
                ApiResponse<object>.Fail(
                    "Product not found",
                    StatusCodes.Status404NotFound));
        }

        _logger.LogInformation(
            "Product stock updated successfully. ProductId: {ProductId}, NewQuantity: {Quantity}",
            id,
            request.StockQuantity);

        return Ok(
            ApiResponse<object>.Ok(
                new
                {
                    ProductId = id,
                    StockQuantity = request.StockQuantity
                },
                "Product stock updated successfully"));
    }
}
