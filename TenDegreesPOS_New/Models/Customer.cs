using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TenDegreesPOS_New.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        public int LoyaltyPoints { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}