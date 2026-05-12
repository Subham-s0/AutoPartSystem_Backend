namespace VehiStock.Application.Dtos.Customer;

public class AppointmentQueryRequest
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? SearchText { get; set; }

    public string? Status { get; set; }

    public string? SortBy { get; set; } = "newest";
}
