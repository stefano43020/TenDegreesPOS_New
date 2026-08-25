using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenDegreesPOS_New.Models
{
    public class OrderCancellation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TransactionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TransactionNumber { get; set; } = string.Empty;

        [Required]
        public DateTime CancelledDate { get; set; } = DateTime.Now;

        [Required]
        public int CancelledByUserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string CancellationType { get; set; } = "Full"; // Full, Partial

        [MaxLength(500)]
        public string? Reason { get; set; }

        public decimal OriginalTotal { get; set; }

        public decimal RefundAmount { get; set; }

        [MaxLength(20)]
        public string RefundMethod { get; set; } = "Original"; // Original, Cash, GCash

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("TransactionId")]
        public virtual Transaction? Transaction { get; set; }

        [ForeignKey("CancelledByUserId")]
        public virtual User? CancelledByUser { get; set; }
    }
}