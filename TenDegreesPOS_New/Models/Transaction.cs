using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenDegreesPOS_New.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string TransactionNumber { get; set; } = string.Empty;

        public int? UserId { get; set; }

        public int? CustomerId { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required]
        public decimal SubTotal { get; set; }

        public decimal Tax { get; set; }

        public decimal Discount { get; set; }

        [Required]
        public decimal Total { get; set; }

        [Required]
        [MaxLength(20)]
        public string PaymentMethod { get; set; } = string.Empty;

        public decimal AmountPaid { get; set; }

        public decimal Change { get; set; }

        public decimal CashPortion { get; set; } = 0;
        public decimal GcashPortion { get; set; } = 0;

        [MaxLength(20)]
        public string? GcashReferenceNumber { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Completed";

        [ForeignKey("UserId")]
        public virtual User? Cashier { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
    }
}