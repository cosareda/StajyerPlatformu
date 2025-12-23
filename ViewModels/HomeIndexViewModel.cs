using StajyerPlatformu.Models;

namespace StajyerPlatformu.ViewModels
{
    public class HomeIndexViewModel
    {
        public IEnumerable<AppUser> Interns { get; set; } = new List<AppUser>();
        public IEnumerable<AppUser> Employers { get; set; } = new List<AppUser>();
    }
}
