using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenDegreesPOS_New.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Barcode { get; set; }

        // Base price (for reference)
        [Required]
        public decimal Price { get; set; }

        // Size-specific pricing
        public decimal? PriceSmall { get; set; }
        public decimal? PriceMedium { get; set; }
        public decimal? PriceLarge { get; set; }

        // Flag to indicate if product has size variants
        public bool HasSizes { get; set; } = false;

        public decimal? Cost { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int CategoryId { get; set; }

        public string? ImagePath { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public ICollection<ProductIngredient> ProductIngredients { get; set; } = new List<ProductIngredient>();
        public ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
        public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
    }
}