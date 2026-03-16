using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftOutsourcing.Models
{
    public class MaterialRequest
    {
        [Key]
        public int Id { get; set; }

        public int AssignmentId { get; set; }
        [ForeignKey("AssignmentId")]
        public Assignment Assignment { get; set; } = null!;

        public int MaterialId { get; set; }
        [ForeignKey("MaterialId")]
        public Material Material { get; set; } = null!;

        public int QuantityRequested { get; set; } // Số lượng nguyên liệu cần cấp thêm

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Completed

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ApprovedDate { get; set; }

        [StringLength(200)]
        public string? Reason { get; set; } // Lý do cấp thêm (ví dụ: "Để làm thêm 50 sản phẩm lỗi")
    }
}
