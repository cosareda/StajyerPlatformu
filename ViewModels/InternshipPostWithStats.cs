using StajyerPlatformu.Models;

namespace StajyerPlatformu.ViewModels
{
    public class InternshipPostWithStats
    {
        public InternshipPost Post { get; set; }
        public int ActiveCandidatesCount { get; set; }
        public int AwaitingReviewCount { get; set; }
        public int ReviewedCount { get; set; }
        public int ContactingCount { get; set; }
        public int HiredCount { get; set; }
        
        // Helper to determine status based on IsActive
        public string Status => Post.IsActive ? "Yayında" : "Pasif";
    }
}
