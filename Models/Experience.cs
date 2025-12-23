using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajyerPlatformu.Models
{
    public class Experience
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Firma adı zorunludur.")]
        [Display(Name = "Firma Adı")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Pozisyon/Başlık zorunludur.")]
        [Display(Name = "Pozisyon")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Toplam Saat")]
        public int TotalHours { get; set; } // For the "Professional hour summary" chart

        public int InternProfileId { get; set; }
        [ForeignKey("InternProfileId")]
        public InternProfile InternProfile { get; set; }
    }
}
