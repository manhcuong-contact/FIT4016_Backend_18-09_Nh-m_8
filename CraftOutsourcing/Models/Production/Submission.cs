using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftOutsourcing.Models
{
    public class Submission
    {
        [Key]
        public int Id { get; set; }

        public int AssignmentId { get; set; }
        [ForeignKey("AssignmentId")]
        public Assignment Assignment { get; set; } = null!;

        public DateTime SubmittedDate { get; set; }

        public int SubmissionNumber { get; set; } // Lần thứ mấy nộp (1, 2, 3...)
        
        public int QuantitySubmitted { get; set; } // Số lượng nộp lần này
        public int QuantityGood { get; set; } // Số lượng thành phẩm đạt KCS
        public int QuantityDefect { get; set; } // Số lượng lỗi

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved (Đã tính lương), Rejected

        [StringLength(200)]
        public string? ReviewNote { get; set; } // Ghi chú khi duyệt

        // One-to-One
        public Payment? Payment { get; set; }
    }
}
