
using OrderManagement.Application.Products.DTOs;

namespace OrderManagement.Application.Interfaces.Services;

public interface IProductService
{
    Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default);

    Task<List<ProductResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<List<ProductResponse>> SearchByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateStockAsync(
        int id,
        UpdateProductStockRequest request,
        CancellationToken cancellationToken = default);
}
