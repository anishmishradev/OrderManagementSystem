
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Interfaces.Repositories;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly OrderManagementDbContext _context;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(
        OrderManagementDbContext context,
        ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Product> AddAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Saving product. SKU: {SKU}",
            product.SKU);

        await _context.Products.AddAsync(
            product, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Product saved successfully. ProductId: {ProductId}, SKU: {SKU}",
            product.Id,
            product.SKU);

        return product;
    }

    public async Task<List<Product>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var products = await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Products retrieved successfully. TotalCount: {Count}",
            products.Count);

        return products;
    }


    public async Task<List<Product>> SearchByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.Name.Contains(name))
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Product search completed. ResultCount: {Count}",
            products.Count);

        return products;
    }
    public async Task<bool> SKUExistsAsync(
        string sku,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.Products
            .AsNoTracking()
            .AnyAsync(p => p.SKU == sku, cancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Product with SKU already exists. SKU: {SKU}",
                sku);
        }

        return exists;
    }

    public async Task<bool> UpdateStockAsync(
        int id,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var affectedRows = await _context.Products
            .Where(p => p.Id == id)
            .ExecuteUpdateAsync(
                x => x.SetProperty(
                    p => p.StockQuantity, quantity),
                cancellationToken);

        if (affectedRows == 0)
        {
            _logger.LogWarning(
                "Stock update failed. Product not found. ProductId: {ProductId}",
                id);

            return false;
        }

        _logger.LogInformation(
            "Product stock updated successfully. ProductId: {ProductId}, NewQuantity: {Quantity}",
            id,
            quantity);

        return true;
    }
}
