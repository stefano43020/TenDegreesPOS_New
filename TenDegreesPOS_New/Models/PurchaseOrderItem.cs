using System.ComponentModel.DataAnnotations;

namespace TenDegreesPOS_New.Models
{
    public class PurchaseOrderItem
    {
        [Key]
        public int Id { get; set; }

        public int PurchaseOrderId { get; set; }

        public int IngredientId { get; set; }

        public string? IngredientName { get; set; }

        public decimal QuantityOrdered { get; set; }  // Original ordered quantity
        public decimal QuantityReceived { get; set; } = 0;  // Quantity actually received
        public bool IsFullyReceived => QuantityReceived >= QuantityOrdered;
        public bool IsPartiallyReceived => QuantityReceived > 0 && QuantityReceived < QuantityOrdered;

        // Box tracking
        public decimal BoxesOrdered { get; set; } = 0;
        public decimal BoxesReceived { get; set; } = 0;
        public decimal? BoxQuantityPerUnit { get; set; } // How many units per box (from ingredient)
        public string? BoxUnit { get; set; } // Unit of the box

        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        // Navigation properties
        public virtual PurchaseOrder? PurchaseOrder { get; set; }
        public virtual Ingredient? Ingredient { get; set; }
    }
}