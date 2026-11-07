using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenDegreesPOS_New.Models
{
    public class ProductIngredient
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int IngredientId { get; set; }

        // Base quantity (default for products without sizes)
        public decimal Quantity { get; set; }

        // Size-specific quantities
        public decimal? QuantitySmall { get; set; }
        public decimal? QuantityMedium { get; set; }
        public decimal? QuantityLarge { get; set; }

        // ▼▼▼ NEW: Track which size this ingredient is for ▼▼▼
        public string? RequiredSize { get; set; } // "Small", "Medium", "Large", or null
        // ▲▲▲ END OF NEW ▲▲▲

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("IngredientId")]
        public virtual Ingredient? Ingredient { get; set; }
    }
}