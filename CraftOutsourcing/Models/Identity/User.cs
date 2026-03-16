using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftOutsourcing.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        public bool IsApproved { get; set; } = true; // Admin moi can duyet, User mac dinh true
        public bool IsActive { get; set; } = true;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public Role Role { get; set; } = null!;

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Penalty> Penalties { get; set; } = new List<Penalty>();
    }
}
