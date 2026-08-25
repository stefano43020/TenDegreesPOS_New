using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TenDegreesPOS_New.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? ContactPerson { get; set; }

        public string? ContactNumber { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? ItemCategory { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<PurchaseOrder>? PurchaseOrders { get; set; }
    }
}