using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("PartCategories")]
public class PartCategory
{
    [Key]
    public int PartCategoryId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<Part> Parts { get; set; } = new List<Part>();
}
