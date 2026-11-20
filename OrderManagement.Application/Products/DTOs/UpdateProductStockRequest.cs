
using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Products.DTOs;

public class UpdateProductStockRequest
{
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}
