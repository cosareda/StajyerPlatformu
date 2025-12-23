using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajyerPlatformu.Models
{
    public class InternProfile
    {
        [Key]
        public int Id { get; set; }

        public string? University { get; set; }
        public string? Department { get; set; }
        
        [Display(Name = "Sınıf")]
        public string? Grade { get; set; } // 1. Sınıf, 2. Sınıf etc.

        [Display(Name = "Öğrenci Numarası")]
        public string? StudentId { get; set; }

        [Display(Name = "Cinsiyet")]
        public string? Gender { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "LinkedIn Profil URL")]
        public string? LinkedIn { get; set; }

        [Display(Name = "Github Profil URL")]
        public string? Github { get; set; }

        public string? ProfilePhotoPath { get; set; }

        public string? Skills { get; set; } // Comma separated for simplicity or JSON
        public string? ResumePath { get; set; }
        
        public ICollection<Experience> Experiences { get; set; } = new List<Experience>();
        
        // Relationship
        public string AppUserId { get; set; }
        [ForeignKey("AppUserId")]
        public AppUser AppUser { get; set; }
    }
}
