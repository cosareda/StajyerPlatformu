using StajyerPlatformu.Models;

namespace StajyerPlatformu.ViewModels
{
    public class EmployerDashboardViewModel
    {
         public EmployerProfile Profile { get; set; }
         public List<InternshipPostWithStats> Posts { get; set; } = new List<InternshipPostWithStats>();
         
         public int TotalMessages { get; set; } = 0; // Dummy for now

    }
}
