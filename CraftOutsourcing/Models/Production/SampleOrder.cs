using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftOutsourcing.Models
{
    public class SampleOrder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string OrderCode { get; set; } = null!; // Ma don hang mau: VD "DH-2026-001"

        [Required]
        [StringLength(200)]
        public string CustomerName { get; set; } = null!; // Khach hang dat

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        public int TotalQuantity { get; set; } // Tong so luong can lam

        public int CompletedQuantity { get; set; } // Số lượng đã hoàn thành

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Confirmed, InProduction, Completed, Cancelled

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? TargetDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedCost { get; set; } // Ươc tính giá thành

        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualCost { get; set; } // Giá thành thực tế

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; } // Gia ban cho khach hang

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
