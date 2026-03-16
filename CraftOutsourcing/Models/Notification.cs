namespace CraftOutsourcing.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = "info"; // info, success, warning, error
        public string? RelatedLink { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
