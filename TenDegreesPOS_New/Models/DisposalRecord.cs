using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenDegreesPOS_New.Models
{
    public class DisposalRecord
    {
        [Key]
        public int Id { get; set; }

        public int IngredientId { get; set; }

        public string IngredientName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public string Unit { get; set; } = string.Empty;

        public DateTime ExpirationDate { get; set; }

        public DateTime DisposalDate { get; set; } = DateTime.Now;

        public int DisposedByUserId { get; set; }

        public string? Reason { get; set; }

        [ForeignKey("IngredientId")]
        public virtual Ingredient? Ingredient { get; set; }

        [ForeignKey("DisposedByUserId")]
        public virtual User? DisposedByUser { get; set; }
    }
}