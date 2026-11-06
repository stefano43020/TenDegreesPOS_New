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

        // --- NEW: Request Details ---
        [Required]
        public DateTime RequestedDate { get; set; } = DateTime.Now;

        [Required]
        public int RequestedByUserId { get; set; }

        // --- NEW: Approval Details ---
        public DateTime? ApprovedDate { get; set; }

        public int? ApprovedByUserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [MaxLength(500)]
        public string? Reason { get; set; }

        public decimal OriginalTotal { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("TransactionId")]
        public virtual Transaction? Transaction { get; set; }

        [ForeignKey("RequestedByUserId")]
        public virtual User? RequestedByUser { get; set; }

        [ForeignKey("ApprovedByUserId")]
        public virtual User? ApprovedByUser { get; set; }
    }
}