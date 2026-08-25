using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenDegreesPOS_New.Models
{
    public class TransactionItem
    {
        [Key]
        public int Id { get; set; }

        public int TransactionId { get; set; }

        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        [MaxLength(10)]
        public string? Size { get; set; } // "Small", "Medium", "Large", or null for no size

        // Navigation properties
        [ForeignKey("TransactionId")]
        public virtual Transaction? Transaction { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}