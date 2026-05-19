using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Admin;

public class CreateVendorRequest
{
    [Required]
    public string VendorCode { get; set; } = string.Empty;

    [Required]
    public string VendorName { get; set; } = string.Empty;

    [Required]
    public string ContactPerson { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;
}
