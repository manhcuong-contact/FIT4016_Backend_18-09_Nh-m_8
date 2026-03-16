using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftOutsourcing.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string Unit { get; set; } = null!; // kg, met, cái...

        public double StockQuantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Giá 1 đơn vị nguyên liệu (dể tính giá thành)

        public double MinStock { get; set; } // Muc ton kho toi thieu de canh bao

        public ICollection<MaterialTransaction> Transactions { get; set; } = new List<MaterialTransaction>();
        public ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();
        public ICollection<AssignmentMaterial> AssignmentMaterials { get; set; } = new List<AssignmentMaterial>();
    }
}
