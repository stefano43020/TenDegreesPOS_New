using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenDegreesPOS_New.Models
{
    public class PurchaseOrderItem
    {
        [Key]
        public int Id { get; set; }

        public int PurchaseOrderId { get; set; }

        public int IngredientId { get; set; }

        public string? IngredientName { get; set; }

        public decimal QuantityOrdered { get; set; }
        public decimal QuantityReceived { get; set; } = 0;
        public bool IsFullyReceived => QuantityReceived >= QuantityOrdered;
        public bool IsPartiallyReceived => QuantityReceived > 0 && QuantityReceived < QuantityOrdered;

        public decimal BoxesOrdered { get; set; } = 0;
        public decimal BoxesReceived { get; set; } = 0;
        public decimal? BoxQuantityPerUnit { get; set; }
        public string? BoxUnit { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        // ▼▼▼ ADD THIS LINE IF IT IS MISSING ▼▼▼
        public decimal ActualUnitCost { get; set; } = 0;
        // ▲▲▲ END OF ADDITION ▲▲▲

        public int? PackagingVariantId { get; set; }

        [ForeignKey("PackagingVariantId")]
        public virtual PackagingVariant? PackagingVariant { get; set; }

        public virtual PurchaseOrder? PurchaseOrder { get; set; }
        public virtual Ingredient? Ingredient { get; set; }
    }
}