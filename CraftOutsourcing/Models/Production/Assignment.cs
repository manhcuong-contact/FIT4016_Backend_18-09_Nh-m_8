using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftOutsourcing.Models
{
    public class Assignment
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; } // Người thợ được giao
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        public int QuantityAssigned { get; set; } // Số lượng sản phẩm yêu cầu làm
        
        public int CompletedQuantity { get; set; } = 0; // Số lượng đã hoàn thành & duyệt được

        public DateTime AssignedDate { get; set; }

        public DateTime DueDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, PendingVerification, Completed, Overdue, Cancelled

        public int? SampleOrderId { get; set; }
        [ForeignKey("SampleOrderId")]
        public SampleOrder? SampleOrder { get; set; }

        public ICollection<AssignmentMaterial> AssignmentMaterials { get; set; } = new List<AssignmentMaterial>();
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
        public ICollection<Penalty> Penalties { get; set; } = new List<Penalty>();
    }
}
