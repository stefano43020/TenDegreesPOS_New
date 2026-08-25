using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenDegreesPOS_New.Models
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        public int? ProductId { get; set; }

        public int? IngredientId { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [Required]
        [MaxLength(10)]
        public string Unit { get; set; } = string.Empty;

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string? Location { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("IngredientId")]
        public virtual Ingredient? Ingredient { get; set; }
    }
}