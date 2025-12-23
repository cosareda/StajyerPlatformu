using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajyerPlatformu.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; } = null!;

        [Required]
        public string ReceiverId { get; set; } = null!;

        [ForeignKey(nameof(SenderId))]
        public AppUser Sender { get; set; } = null!;

        [ForeignKey(nameof(ReceiverId))]
        public AppUser Receiver { get; set; } = null!;

        [MaxLength(200)]
        public string? Subject { get; set; }

        [Required]
        public string Content { get; set; } = null!;

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }
}


