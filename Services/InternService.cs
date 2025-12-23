using Microsoft.EntityFrameworkCore;
using StajyerPlatformu.Data;
using StajyerPlatformu.Models;

namespace StajyerPlatformu.Services
{
    public class InternService : IInternService
    {
        private readonly AppDbContext _context;

        public InternService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<InternProfile?> GetProfileAsync(string userId)
        {
            return await _context.InternProfiles
                .Include(p => p.AppUser)
                .Include(p => p.Experiences)
                .FirstOrDefaultAsync(p => p.AppUserId == userId);
        }

        public async Task<List<InternshipPost>> SearchJobsAsync(string? searchString, string? city, string? workType, string? companyName)
        {
            var posts = _context.InternshipPosts
                .Include(p => p.EmployerProfile)
                .Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(searchString))
            {
                posts = posts.Where(s => s.Title.Contains(searchString) || s.Description.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(city))
            {
                posts = posts.Where(p => p.City == city);
            }

            if (!string.IsNullOrEmpty(workType))
            {
                posts = posts.Where(p => p.WorkType == workType);
            }

            if (!string.IsNullOrEmpty(companyName))
            {
                posts = posts.Where(p => p.EmployerProfile != null && p.EmployerProfile.CompanyName.Contains(companyName));
            }

            return await posts.ToListAsync();
        }

        public async Task<List<string>> GetCitiesAsync()
        {
            return await _context.InternshipPosts
                .Select(p => p.City)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<string>> GetCompaniesAsync()
        {
            return await _context.InternshipPosts
                .Include(p => p.EmployerProfile)
                .Where(p => p.EmployerProfile != null)
                .Select(p => p.EmployerProfile!.CompanyName)
                .Distinct()
                .ToListAsync();
        }

        public async Task<InternProfile?> GetProfileForEditAsync(string userId)
        {
            return await _context.InternProfiles
                .FirstOrDefaultAsync(p => p.AppUserId == userId);
        }

        public async Task CreateOrUpdateProfileAsync(InternProfile model, string userId, string? profilePhotoPath, string? resumePath)
        {
            var existingProfile = await _context.InternProfiles
                .FirstOrDefaultAsync(p => p.AppUserId == userId);

            if (existingProfile == null)
            {
                model.AppUserId = userId;
                if (!string.IsNullOrEmpty(profilePhotoPath))
                    model.ProfilePhotoPath = profilePhotoPath;
                if (!string.IsNullOrEmpty(resumePath))
                    model.ResumePath = resumePath;
                _context.InternProfiles.Add(model);
            }
            else
            {
                existingProfile.University = model.University;
                existingProfile.Department = model.Department;
                existingProfile.Grade = model.Grade;
                existingProfile.StudentId = model.StudentId;
                existingProfile.Gender = model.Gender;
                existingProfile.BirthDate = model.BirthDate;
                existingProfile.LinkedIn = model.LinkedIn;
                existingProfile.Github = model.Github;
                existingProfile.Skills = model.Skills;

                if (!string.IsNullOrEmpty(profilePhotoPath))
                    existingProfile.ProfilePhotoPath = profilePhotoPath;

                if (!string.IsNullOrEmpty(resumePath))
                    existingProfile.ResumePath = resumePath;

                _context.Update(existingProfile);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> AddExperienceAsync(Experience experience, string userId)
        {
            var profile = await _context.InternProfiles
                .FirstOrDefaultAsync(p => p.AppUserId == userId);

            if (profile == null) return false;

            experience.InternProfileId = profile.Id;
            _context.Experiences.Add(experience);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteExperienceAsync(int experienceId)
        {
            var exp = await _context.Experiences.FindAsync(experienceId);
            if (exp != null)
            {
                _context.Experiences.Remove(exp);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<InternshipPost?> GetJobDetailAsync(int jobId)
        {
            return await _context.InternshipPosts
                .Include(p => p.EmployerProfile)
                .ThenInclude(e => e!.AppUser)
                .Include(p => p.Applications)
                .FirstOrDefaultAsync(p => p.Id == jobId && p.IsActive);
        }

        public async Task<bool> CheckIfAppliedAsync(int jobId, string userId)
        {
            var profile = await _context.InternProfiles
                .FirstOrDefaultAsync(p => p.AppUserId == userId);

            if (profile == null) return false;

            return await _context.Applications
                .AnyAsync(a => a.InternshipPostId == jobId && a.InternProfileId == profile.Id);
        }

        public async Task<bool> ApplyToJobAsync(int jobId, string userId)
        {
            var profile = await _context.InternProfiles
                .FirstOrDefaultAsync(p => p.AppUserId == userId);

            if (profile == null) return false;

            var post = await _context.InternshipPosts.FindAsync(jobId);
            if (post == null || !post.IsActive) return false;

            var exists = await _context.Applications
                .AnyAsync(a => a.InternshipPostId == jobId && a.InternProfileId == profile.Id);

            if (exists) return false;

            var application = new Application
            {
                InternshipPostId = jobId,
                InternProfileId = profile.Id,
                ApplicationDate = DateTime.Now,
                Status = ApplicationStatus.Pending
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Application>> GetMyApplicationsAsync(string userId)
        {
            var profile = await _context.InternProfiles
                .FirstOrDefaultAsync(p => p.AppUserId == userId);

            if (profile == null) return new List<Application>();

            return await _context.Applications
                .Include(a => a.InternshipPost)
                .ThenInclude(p => p!.EmployerProfile)
                .Where(a => a.InternProfileId == profile.Id)
                .ToListAsync();
        }
    }
}

