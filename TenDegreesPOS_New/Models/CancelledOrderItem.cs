using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenDegreesPOS_New.Models
{
    public class CancelledOrderItem
    {
        [Key]
        public int Id { get; set; }

        public int CancellationId { get; set; }

        public int OriginalTransactionItemId { get; set; }

        public int ProductId { get; set; }

        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        [MaxLength(10)]
        public string? Size { get; set; }

        [ForeignKey("CancellationId")]
        public virtual OrderCancellation? Cancellation { get; set; }
    }
}