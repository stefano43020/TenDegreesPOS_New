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

        public bool IsDeductedFromStock { get; set; } = false;

        public DateTime ExpirationDate { get; set; }

        public decimal UnitCost { get; set; } = 0; // The price paid per pack for this batch

        public DateTime ReceivedDate { get; set; } = DateTime.Now;

        [ForeignKey("InventoryId")]
        public virtual Inventory? Inventory { get; set; }
    }
}