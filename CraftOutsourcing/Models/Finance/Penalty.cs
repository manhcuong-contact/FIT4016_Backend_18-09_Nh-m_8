using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftOutsourcing.Models
{
    public class Penalty
    {
        [Key]
        public int Id { get; set; }

        public int AssignmentId { get; set; }
        [ForeignKey("AssignmentId")]
        public Assignment Assignment { get; set; } = null!;

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string Reason { get; set; } = null!; // Overdue, QualityFail

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Waived, Deducted, Paid, Resolved

        // Ngày thanh toán phạt (nếu hộ dân đã đóng phạt)
        public DateTime? PaidDate { get; set; }

        // Tracking defect resolution: số lượng lỗi liên quan đến penalty này
        public int DefectiveQuantity { get; set; } = 0;

        // Submissio ID liên quan (để truy vết nguồn lỗi)
        public int? SubmissionId { get; set; }
        [ForeignKey("SubmissionId")]
        public Submission? Submission { get; set; }
    }
}
