using Microsoft.AspNetCore.Http;

namespace VehiStock.Application.DTOs
{
    public class CreatePartDto
    {
        public int PartCategoryId { get; set; }
        public string PartCode { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public decimal UnitCost { get; set; }
        public decimal UnitPrice { get; set; }
        public int StockQty { get; set; }
        public int MinimumStock { get; set; }
        public IFormFile? PartPhoto { get; set; }
    }
}