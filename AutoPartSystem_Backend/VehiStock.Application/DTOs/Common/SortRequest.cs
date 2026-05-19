namespace VehiStock.Application.Dtos.Common;

public class SortRequest
{
    public string SortBy { get; set; } = string.Empty;

    public SortDirection SortDirection { get; set; } = SortDirection.Desc;
}

public enum SortDirection
{
    Asc,
    Desc
}
