namespace VehiStock.Application.Dtos.Staff;

public class CustomerDeskHistoryLineResponse
{
    public string PartName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime Date { get; set; }
}
