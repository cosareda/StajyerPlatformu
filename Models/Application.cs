using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajyerPlatformu.Models
{
    public enum ApplicationStatus
    {
        Pending,        // Beklemede
        InReview,       // İnceleniyor
        Accepted,       // Kabul
        Rejected        // Reddedildi
    }

    public class Application
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime ApplicationDate { get; set; } = DateTime.Now;
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        public int InternshipPostId { get; set; }
        public InternshipPost InternshipPost { get; set; }

        public int InternProfileId { get; set; }
        public InternProfile InternProfile { get; set; }
    }
}
