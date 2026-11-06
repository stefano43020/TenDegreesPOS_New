using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenDegreesPOS_New.Models;

public class PackagingVariant
{
    [Key]
    public int Id { get; set; }

    public int IngredientId { get; set; }

    [Required]
    [MaxLength(50)]
    public string VariantName { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    [MaxLength(10)]
    public string Unit { get; set; } = "g";

    // ▼▼▼ NEW: Price of this specific package ▼▼▼
    public decimal Cost { get; set; } = 0;
    // ▲▲▲ END ▲▲▲

    [ForeignKey("IngredientId")]
    public virtual Ingredient? Ingredient { get; set; }
}