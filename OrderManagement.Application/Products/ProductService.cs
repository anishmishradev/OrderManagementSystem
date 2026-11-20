
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Interfaces.Repositories;
using OrderManagement.Application.Interfaces.Services;
using OrderManagement.Application.Products.DTOs;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Products;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        var sku = request.SKU?.Trim().ToUpperInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException(
                "Product name and SKU are required.");
        }

        if (name.Length > 150 || sku.Length > 50)
        {
            throw new ArgumentException(
                "Product name or SKU exceeds allowed length.");
        }

        if (request.Price <= 0 || request.StockQuantity < 0)
        {
            throw new ArgumentException(
                "Invalid product price or stock quantity.");
        }

        if (await _repository.SKUExistsAsync(
            sku, cancellationToken))
        {
            _logger.LogWarning(
                "Duplicate product SKU detected. SKU: {SKU}",
                sku);

            throw new InvalidOperationException(
                "Product SKU already exists.");
        }

        var product = new Product
        {
            Name = name,
            SKU = sku,
            Price = request.Price,
            StockQuantity = request.StockQuantity
        };

        var result = await _repository.AddAsync(
            product, cancellationToken);

        _logger.LogInformation(
            "Product created successfully. ProductId: {ProductId}",
            result.Id);

        return Map(result);
    }

    public async Task<List<ProductResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetAllAsync(
            cancellationToken);

        return products.Select(Map).ToList();
    }

    public async Task<List<ProductResponse>> SearchByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Product name is required.");
        }

        var searchTerm = name.Trim();

        if (searchTerm.Length > 150)
        {
            throw new ArgumentException(
                "Search term cannot exceed 150 characters.");
        }

        var products = await _repository.SearchByNameAsync(
            searchTerm, cancellationToken);

        return products.Select(Map).ToList();
    }

    public async Task<bool> UpdateStockAsync(
        int id,
        UpdateProductStockRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0 || request.StockQuantity < 0)
        {
            throw new ArgumentException(
                "Invalid product ID or stock quantity.");
        }

        var updated = await _repository.UpdateStockAsync(
            id,
            request.StockQuantity,
            cancellationToken);

        if (updated)
        {
            _logger.LogInformation(
                "Product stock updated successfully. ProductId: {ProductId}, Quantity: {Quantity}",
                id,
                request.StockQuantity);
        }

        return updated;
    }

    private static ProductResponse Map(Product product)
    {
        return new ProductResponse(
            product.Id,
            product.Name,
            product.SKU,
            product.Price,
            product.StockQuantity);
    }
}
