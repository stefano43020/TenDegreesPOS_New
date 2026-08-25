using System;
using System.ComponentModel.DataAnnotations;

namespace TenDegreesPOS_New.Models
{
    public class StoreSettings
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string StoreName { get; set; } = "Ten Degrees POS";

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        public string? TaxNumber { get; set; }

        public decimal TaxRate { get; set; } = 8.25m;

        public bool TaxInclusive { get; set; } = false;

        [MaxLength(20)]
        public string Currency { get; set; } = "₱";

        [MaxLength(20)]
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public bool ReceiptPrinting { get; set; } = true;

        public string? ReceiptFooter { get; set; }

        public DateTime LastBackup { get; set; } = DateTime.Now;

        public bool AutoBackup { get; set; } = false;

        public int SessionTimeout { get; set; } = 30;
    }
}