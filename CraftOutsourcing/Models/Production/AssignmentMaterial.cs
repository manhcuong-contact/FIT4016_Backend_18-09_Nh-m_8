using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftOutsourcing.Models
{
    public class AssignmentMaterial
    {
        [Key]
        public int Id { get; set; }

        public int AssignmentId { get; set; }
        [ForeignKey("AssignmentId")]
        public Assignment Assignment { get; set; } = null!;

        public int MaterialId { get; set; }
        [ForeignKey("MaterialId")]
        public Material Material { get; set; } = null!;

        public double QuantityGiven { get; set; } // Số lượng nguyên liệu thực tế cấp cho thợ
    }
}
