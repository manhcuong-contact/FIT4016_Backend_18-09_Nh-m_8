using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftOutsourcing.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; } // Phải trả cho người thợ nào
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public int SubmissionId { get; set; }
        [ForeignKey("SubmissionId")]
        public Submission Submission { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // Tổng tiền công

        public DateTime PaymentDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Paid"; // Unpaid, Paid
    }
}
