
using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Customers.DTOs;

public class CreateCustomerRequest
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(150, MinimumLength = 2)]
    [RegularExpression(@"^[a-zA-Z ]+$",ErrorMessage = "Full name must contain only alphabets.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;
}
