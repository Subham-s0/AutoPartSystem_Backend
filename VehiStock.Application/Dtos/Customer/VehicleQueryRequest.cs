using VehiStock.Application.Dtos.Common;

namespace VehiStock.Application.Dtos.Customer;

public class VehicleQueryRequest
{
    public string? SearchText { get; set; }

    public List<SortRequest> Sorts { get; set; } = [];
}
