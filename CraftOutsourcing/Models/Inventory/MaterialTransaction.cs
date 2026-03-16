using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftOutsourcing.Models
{
    public class MaterialTransaction
    {
        [Key]
        public int Id { get; set; }

        public int MaterialId { get; set; }
        [ForeignKey("MaterialId")]
        public Material Material { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string TransactionType { get; set; } = null!; // Import (Nhập), Export (Xuất cho thợ)

        public double Quantity { get; set; }

        public DateTime TransactionDate { get; set; }

        // Lưu vết lại Id của Assignment nếu xuất kho để làm Assignment đó
        public int? ReferenceId { get; set; }
    }
}
