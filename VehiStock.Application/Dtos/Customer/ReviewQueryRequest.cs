using VehiStock.Application.Dtos.Common;

namespace VehiStock.Application.Dtos.Customer;

public class ReviewQueryRequest
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 9;

    public string? SearchText { get; set; }

    public int? Rating { get; set; }

    public List<SortRequest> Sorts { get; set; } = [];
}
