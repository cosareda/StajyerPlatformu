using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajyerPlatformu.Models
{
    public enum InterviewStatus
    {
        Planned = 0,
        Completed = 1,
        Canceled = 2
    }

    public class Interview
    {
        public int Id { get; set; }

        public int EmployerProfileId { get; set; }
        public EmployerProfile EmployerProfile { get; set; } = null!;

        public int InternProfileId { get; set; }
        public InternProfile InternProfile { get; set; } = null!;

        public int InternshipPostId { get; set; }
        public InternshipPost InternshipPost { get; set; } = null!;

        [DataType(DataType.DateTime)]
        public DateTime ScheduledAt { get; set; }

        [MaxLength(200)]
        public string Location { get; set; } = "Online";

        [MaxLength(500)]
        public string? Note { get; set; }

        public InterviewStatus Status { get; set; } = InterviewStatus.Planned;
    }
}


