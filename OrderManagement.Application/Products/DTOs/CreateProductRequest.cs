
using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Products.DTOs;

public class CreateProductRequest
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999.99")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}
