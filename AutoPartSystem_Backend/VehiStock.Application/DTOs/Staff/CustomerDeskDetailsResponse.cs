using System.Text.Json.Serialization;

namespace VehiStock.Application.Dtos.Staff;

public class CustomerDeskDetailsResponse
{
    public int CustomerId { get; set; }

    [JsonPropertyName("fullname")]
    public string Fullname { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public IReadOnlyList<string>? Vehicles { get; set; }
}
