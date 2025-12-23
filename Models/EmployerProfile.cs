using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajyerPlatformu.Models
{
    public class EmployerProfile
    {
        [Key]
        public int Id { get; set; }

        public string CompanyName { get; set; }
        public string? Sector { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        
        [Display(Name = "Şehir / Lokasyon")]
        public string? City { get; set; }

        public string? LogoPath { get; set; }

        // Relationship
        public string AppUserId { get; set; }
        [ForeignKey("AppUserId")]
        public AppUser AppUser { get; set; }
    }
}
