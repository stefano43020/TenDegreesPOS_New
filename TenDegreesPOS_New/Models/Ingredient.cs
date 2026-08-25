using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TenDegreesPOS_New.Models
{
    public class Ingredient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Unit { get; set; } = "g";

        // Box/Bulk settings
        public bool IsSoldInBoxes { get; set; } = false;
        public decimal BoxQuantity { get; set; } = 1;
        public string? BoxUnit { get; set; } = "kg";
        public string? BoxDisplayName { get; set; } = "Box";

        // NEW: Conversion to base unit (grams or milliliters)
        public string? BaseUnit { get; set; } = "g"; // Base unit for this ingredient (g or ml)
        public decimal GramsPerUnit { get; set; } = 1; // For weight-based ingredients
        public decimal MlPerUnit { get; set; } = 1;    // For volume-based ingredients

        public int MinimumStock { get; set; } = 10;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<ProductIngredient>? ProductIngredients { get; set; }
        public virtual ICollection<Inventory>? Inventory { get; set; }
    }
}