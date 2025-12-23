using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajyerPlatformu.Models
{
    public class InternshipPost
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime Deadline { get; set; }
        public bool IsActive { get; set; } = true;

        [Required]
        public string City { get; set; } // Istanbul, Ankara vs.

        [Display(Name = "Çalışma Türü")]
        public string WorkType { get; set; } // Remote, Hybrid, Onsite

        [Display(Name = "Staj Süresi")]
        public string? Duration { get; set; } // 20 gün, 3 ay vb.

        // Foreign Key to Employer
        public int EmployerProfileId { get; set; }
        public EmployerProfile EmployerProfile { get; set; }

        public ICollection<Application> Applications { get; set; }
    }
}
