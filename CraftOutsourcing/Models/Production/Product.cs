using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftOutsourcing.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Đơn giá trả cho thợ trên 1 sản phẩm hoàn thành

        public int FinishedStock { get; set; } // Tồn kho thành phẩm

        public ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<SampleOrder> SampleOrders { get; set; } = new List<SampleOrder>();
    }
}
