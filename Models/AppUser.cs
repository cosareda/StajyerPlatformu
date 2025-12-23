using Microsoft.AspNetCore.Identity;

namespace StajyerPlatformu.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsApproved { get; set; } = false; // New users are unapproved by default
    }
}
