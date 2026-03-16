using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftOutsourcing.Models
{
    public class ProductMaterial
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        public int MaterialId { get; set; }
        [ForeignKey("MaterialId")]
        public Material Material { get; set; } = null!;

        public double QuantityRequired { get; set; } // Định mức - Số lượng nguyên liệu cần cho 1 sản phẩm
    }
}
