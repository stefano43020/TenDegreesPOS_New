using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TenDegreesPOS_New.Models
{
    public class PurchaseOrder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string OrderNumber { get; set; } = string.Empty;

        public int SupplierId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public DateTime? ExpectedDeliveryDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string? Status { get; set; } = "Processing"; // Processing, Completed, Cancelled

        public string? Notes { get; set; }

        public int CreatedBy { get; set; } // User ID

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Supplier? Supplier { get; set; }
        public virtual User? CreatedByUser { get; set; }
        public virtual ICollection<PurchaseOrderItem>? PurchaseOrderItems { get; set; }
    }
}