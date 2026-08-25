using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenDegreesPOS_New.Models
{
    public class SupplierIngredient
    {
        [Key]
        public int Id { get; set; }

        public int SupplierId { get; set; }

        public int IngredientId { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier? Supplier { get; set; }

        [ForeignKey("IngredientId")]
        public virtual Ingredient? Ingredient { get; set; }
    }
}