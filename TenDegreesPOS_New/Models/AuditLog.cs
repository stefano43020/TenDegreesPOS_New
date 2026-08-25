using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenDegreesPOS_New.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? TableName { get; set; }

        public int? RecordId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}