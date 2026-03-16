using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CraftOutsourcing.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
