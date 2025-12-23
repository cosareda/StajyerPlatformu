using StajyerPlatformu.Models;

namespace StajyerPlatformu.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalInterns { get; set; }
        public int TotalEmployers { get; set; }
        public int PendingApprovals { get; set; }
        public int TotalApplications { get; set; }
        public int TotalJobs { get; set; }
        
        public List<AppUser> PendingUsers { get; set; } = new List<AppUser>();
        public List<InternshipPost> RecentJobs { get; set; } = new List<InternshipPost>();
    }
}
