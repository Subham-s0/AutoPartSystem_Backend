namespace VehiStock.Application.Dtos.Staff;

public class ServiceRecordPartResponse
{
    public int ServiceRecordPartId { get; init; }
    public int PartId { get; init; }
    public string PartName { get; init; } = string.Empty;
    public string Brand { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}
