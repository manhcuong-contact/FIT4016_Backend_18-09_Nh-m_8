using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftOutsourcing.Models
{
    public class Profit
    {
        [Key]
        public int Id { get; set; }

        public int SampleOrderId { get; set; }
        [ForeignKey("SampleOrderId")]
        public SampleOrder SampleOrder { get; set; } = null!;

        // Số lượng sản phẩm hoàn thành tốt (dùng để bán)
        public int QuantityGood { get; set; }

        // Số lượng sản phẩm lỗi (không bán được)
        public int QuantityDefect { get; set; }

        // Giá bán trên một sản phẩm
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }

        // Giá vốn (chi phí nguyên liệu) cho một sản phẩm
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; }

        // Lợi nhuận từ bán sản phẩm = (QuantityGood * SellingPrice) - (QuantityGood * CostPrice)
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalesProfit { get; set; } // = (QuantityGood * SellingPrice) - (QuantityGood * CostPrice)

        // Tiền phạt nhận được từ sản phẩm lỗi (vì hộ gia công phải đền)
        [Column(TypeName = "decimal(18,2)")]
        public decimal PenaltyRevenue { get; set; } // = QuantityDefect * PenaltyAmountPerItem

        // Tổng lợi nhuận = SalesProfit + PenaltyRevenue
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalProfit { get; set; }

        public DateTime RecordDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Cancelled
    }
}
