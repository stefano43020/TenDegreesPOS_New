using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenDegreesPOS_New.Models
{
    public class InventoryBatch
    {
        [Key]
        public int Id { get; set; }

        public int InventoryId { get; set; }

        public decimal Quantity { get; set; }

        public DateTime ExpirationDate { get; set; }

        public DateTime ReceivedDate { get; set; } = DateTime.Now;

        [ForeignKey("InventoryId")]
        public virtual Inventory? Inventory { get; set; }
    }
}