using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StajyerPlatformu.Models;

namespace StajyerPlatformu.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<InternProfile> InternProfiles { get; set; }
        public DbSet<EmployerProfile> EmployerProfiles { get; set; }
        public DbSet<InternshipPost> InternshipPosts { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Experience> Experiences { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Interview> Interviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships if needed more explicitly
            
            // Example: One Employer has many Posts
            builder.Entity<InternshipPost>()
                .HasOne(p => p.EmployerProfile)
                .WithMany()
                .HasForeignKey(p => p.EmployerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // AppUser -> One InternProfile (Optional)
            builder.Entity<AppUser>()
                .HasOne<InternProfile>()
                .WithOne(i => i.AppUser)
                .HasForeignKey<InternProfile>(i => i.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // AppUser -> One EmployerProfile (Optional)
             builder.Entity<AppUser>()
                .HasOne<EmployerProfile>()
                .WithOne(e => e.AppUser)
                .HasForeignKey<EmployerProfile>(e => e.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
